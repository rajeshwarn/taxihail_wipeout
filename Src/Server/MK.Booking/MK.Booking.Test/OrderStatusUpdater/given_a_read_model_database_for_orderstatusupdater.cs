using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Booking.Test.Integration;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using CMTServices;
using CustomerPortal.Client.Impl;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.OrderStatusUpdater
{
    public class given_a_read_model_database_for_orderstatusupdater : given_a_read_model_database
    {
        protected IList<ICommand> Commands; 

        protected TestServerSettings ConfigurationManager;
        protected Mock<IPaymentService> PaymentServiceMock;
        protected Mock<IIbsOrderService> IbsOrderServiceMock;
        protected Mock<INotificationService> NotificationServiceMock;
        protected Mock<ILogger> LoggerMock;
        protected Jobs.OrderStatusUpdater Sut { get; set; }

        [SetUp]
        public void Setup()
        {
            Commands = new List<ICommand>();

            ConfigurationManager = new TestServerSettings();

            PaymentServiceMock = new Mock<IPaymentService>();
            PaymentServiceMock
                .Setup(x => x.IsPayPal(It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.Is<bool>(o => false)))
                .Returns<Guid?, Guid?, bool>(IsPayPal);
            PaymentServiceMock
                .Setup(x => x.ProviderType(It.IsAny<string>(), It.IsAny<Guid?>()))
                .Returns<string, Guid?>(ProviderType);

            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            IbsOrderServiceMock = new Mock<IIbsOrderService>();

            NotificationServiceMock = new Mock<INotificationService>();

            LoggerMock = new Mock<ILogger>();
            LoggerMock.Setup(x => x.LogError(It.IsAny<Exception>())).Callback<Exception>(x => Console.WriteLine(x.ToString()));
            LoggerMock.Setup(x => x.LogMessage(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>((s, paramobjects) => Console.WriteLine(paramobjects != null ? string.Format(s, paramobjects) : s));

            var accountDao = new AccountDao(() => new BookingDbContext(DbName));
            var orderDao = new OrderDao(() => new BookingDbContext(DbName));
            var orderPaymentDao = new OrderPaymentDao(() => new BookingDbContext(DbName));

            var notificationDetailsDaoMock = new Mock<IOrderNotificationsDetailDao>(MockBehavior.Loose);
            var taxihailNetworkServiceClient = new TaxiHailNetworkServiceClient(ConfigurationManager);

            Sut = new Jobs.OrderStatusUpdater(ConfigurationManager,
                bus.Object,
                orderPaymentDao,
                orderDao,
                NotificationServiceMock.Object,
                null,
                accountDao,
                IbsOrderServiceMock.Object,
                new PromotionDao(() => new BookingDbContext(DbName), new SystemClock(), ConfigurationManager, null),
                null,
                PaymentServiceMock.Object,
                new CreditCardDao(() => new BookingDbContext(DbName)),
                new FeeService(PaymentServiceMock.Object, accountDao, new FeesDao(() => new BookingDbContext(DbName)), orderDao, orderPaymentDao, bus.Object, ConfigurationManager, LoggerMock.Object),
                notificationDetailsDaoMock.Object,
                new CmtGeoServiceClient(ConfigurationManager, LoggerMock.Object),
                new DispatcherService(LoggerMock.Object, new IBSServiceProvider(ConfigurationManager, LoggerMock.Object, taxihailNetworkServiceClient), ConfigurationManager, bus.Object, taxihailNetworkServiceClient, accountDao, orderDao),
                new VehicleTypeDao(() => new BookingDbContext(DbName)), 
                new IBSServiceProvider(ConfigurationManager, LoggerMock.Object, taxihailNetworkServiceClient),
                taxihailNetworkServiceClient,
                LoggerMock.Object);
        }
        
        protected void EnsurePreAuthPaymentForTripWasCalled(OrderStatusDetail status, decimal amount)
        {
            var isReAuth = false;

            PaymentServiceMock
                .Setup(x => x.PreAuthorize(
                    It.Is<string>(o => o == status.CompanyKey),
                    It.Is<Guid>(o => o == status.OrderId),
                    It.Is<AccountDetail>(o => o.Id == status.AccountId),
                    It.Is<decimal>(o => o == amount),
                    It.Is<bool>(o => o == isReAuth),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>()))
                .Returns<string, Guid, AccountDetail, decimal, bool, bool, bool, string>(PreAuth)
                .Verifiable();
        }

        protected void EnsureCommitWasCalled(OrderStatusDetail status, decimal preauthAmount, decimal totalAmount, decimal meterAmount, decimal tipAmount)
        {
            PaymentServiceMock
                .Setup(x => x.CommitPayment(
                    It.Is<string>(o => o == status.CompanyKey),
                    It.Is<Guid>(o => o == status.OrderId),
                    It.Is<AccountDetail>(o => o.Id == status.AccountId),
                    It.Is<decimal>(o => o == preauthAmount),
                    It.Is<decimal>(o => o == totalAmount),
                    It.Is<decimal>(o => o == meterAmount),
                    It.Is<decimal>(o => o == tipAmount),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns<string, Guid, AccountDetail, decimal, decimal, decimal, decimal, string, string, bool>(Commit)
                .Verifiable();
        }

        protected void EnsureVoidPreAuthWasCalled(OrderStatusDetail status)
        {
            PaymentServiceMock
                .Setup(x => x.VoidPreAuthorization(
                    It.Is<string>(o => o == status.CompanyKey),
                    It.Is<Guid>(o => o == status.OrderId),
                    It.IsAny<bool>()))
                .Verifiable();
        }

        protected void EnsurePreAuthForFeeWasCalled(OrderStatusDetail status, decimal feeAmount)
        {
            var isReAuth = false;

            PaymentServiceMock
                .Setup(x => x.PreAuthorize(
                    It.Is<string>(o => o == null),
                    It.Is<Guid>(o => o == status.OrderId),
                    It.Is<AccountDetail>(o => o.Id == status.AccountId),
                    It.Is<decimal>(o => o == feeAmount),
                    It.Is<bool>(o => o == isReAuth),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>()))
                .Returns<string, Guid, AccountDetail, decimal, bool, bool, bool, string>(PreAuth)
                .Verifiable();
        }

        protected void EnsureCommitForFeeWasCalled(OrderStatusDetail status, decimal feeAmount, decimal preAuthAmount)
        {
            PaymentServiceMock
                .Setup(x => x.CommitPayment(
                    It.Is<string>(o => o == null),
                    It.Is<Guid>(o => o == status.OrderId),
                    It.Is<AccountDetail>(o => o.Id == status.AccountId),
                    It.Is<decimal>(o => o == preAuthAmount),
                    It.Is<decimal>(o => o == feeAmount),
                    It.Is<decimal>(o => o == feeAmount),
                    It.Is<decimal>(o => o == 0),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns<string, Guid, AccountDetail, decimal, decimal, decimal, decimal, string, string, bool>(Commit)
                .Verifiable();
        }

        protected void EnsureVoidPreAuthForFeeWasCalled(OrderStatusDetail status)
        {
            PaymentServiceMock
                .Setup(x => x.VoidPreAuthorization(
                    It.Is<string>(o => o == null),
                    It.Is<Guid>(o => o == status.OrderId),
                    It.IsAny<bool>()))
                .Verifiable();
        }

        private bool IsPayPal(Guid? accountId, Guid? orderId, bool isForPrepaid)
        {
            var paymentService = new PaymentService(null, new AccountDao(() => new BookingDbContext(DbName)), new OrderDao(() => new BookingDbContext(DbName)), null, ConfigurationManager, null);
            return paymentService.IsPayPal(accountId, orderId, isForPrepaid);
        }

        private PaymentProvider ProviderType(string companyKey, Guid? orderId)
        {
            if (IsPayPal(null, orderId, false))
            {
                return PaymentProvider.PayPal;
            }

            var paymentMode = ConfigurationManager.GetPaymentSettings(companyKey).PaymentMode;
            switch (paymentMode)
            {
                case PaymentMethod.Cmt:
                case PaymentMethod.RideLinqCmt:
                    return PaymentProvider.Cmt;
                case PaymentMethod.Braintree:
                    return PaymentProvider.Braintree;
                case PaymentMethod.Moneris:
                    return PaymentProvider.Moneris;
                default:
                    throw new Exception();
            }
        }

        protected PreAuthorizePaymentResponse PreAuth(string companyKey, Guid orderId, AccountDetail account, decimal amountToPreAuthorize, bool isReAuth, bool isSettlingOverduePayment, bool isForPrepaid, string cvv)
        {
            var pennyValue = amountToPreAuthorize - Math.Truncate(amountToPreAuthorize);
            pennyValue = pennyValue * 100;

            if (pennyValue <= 50)
            {
                var transactionId = "asdasdnasd";

                using (var context = new BookingDbContext(DbName))
                {
                    // must create an order payment detail when preauth works
                    context.Save(new OrderPaymentDetail
                    {
                        PaymentId = Guid.NewGuid(),
                        PreAuthorizedAmount = amountToPreAuthorize,
                        FirstPreAuthTransactionId = transactionId,
                        TransactionId = transactionId,
                        OrderId = orderId,
                        CardToken = "token",
                        CompanyKey = companyKey
                    });
                }

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = true,
                    ReAuthOrderId = isReAuth ? Guid.NewGuid().ToString() : null,
                    TransactionDate = DateTime.Now,
                    TransactionId = transactionId
                };
            }

            if (pennyValue <= 75)
            {
                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = false,
                    ReAuthOrderId = isReAuth ? Guid.NewGuid().ToString() : null,
                    TransactionDate = DateTime.Now,
                    TransactionId = "asdasdnasd"
                };
            }

            return new PreAuthorizePaymentResponse
            {
                IsSuccessful = false,
                IsDeclined = true,
                ReAuthOrderId = isReAuth ? Guid.NewGuid().ToString() : null,
                TransactionDate = DateTime.Now,
                TransactionId = "asdasdnasd",
                Message = "PROCESSOR DECLINED"
            };
        }

        protected CommitPreauthorizedPaymentResponse Commit(string companyKey, Guid orderId, AccountDetail account, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId, string reAuthOrderId = null, bool isForPrepaid = false)
        {
            var pennyValue = amount - Math.Truncate(amount);
            pennyValue = pennyValue * 100;

            if (pennyValue <= 50)
            {
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = true,
                    TransactionDate = DateTime.Now,
                    TransactionId = "asdasdnasd",
                    AuthorizationCode = "5236346"
                };
            }

            if (pennyValue <= 75)
            {
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = false,
                    TransactionDate = DateTime.Now,
                    TransactionId = "asdasdnasd",
                    AuthorizationCode = "5236346"
                };
            }

            return new CommitPreauthorizedPaymentResponse
            {
                IsSuccessful = false,
                IsDeclined = true,
                TransactionDate = DateTime.Now,
                TransactionId = "asdasdnasd",
                AuthorizationCode = "5236346",
                Message = "PROCESSOR DECLINED"
            };
        }
    }
}
using System;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class ProcessPaymentService : Service
    {
        private static string _failedCode = "0";

        private readonly IPayPalServiceFactory _payPalServiceFactory;
        private readonly IPaymentServiceFactory _paymentServiceFactory;
        private readonly IAccountDao _accountDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IOrderDao _orderDao;
        private readonly IServerSettings _serverSettings;

        public ProcessPaymentService(
            IPayPalServiceFactory payPalServiceFactory,
            IPaymentServiceFactory paymentServiceFactory, 
            IAccountDao accountDao, 
            IOrderDao orderDao,
            IIBSServiceProvider ibsServiceProvider,
            IServerSettings serverSettings)
        {
            _payPalServiceFactory = payPalServiceFactory;
            _paymentServiceFactory = paymentServiceFactory;
            _accountDao = accountDao;
            _orderDao = orderDao;
            _ibsServiceProvider = ibsServiceProvider;
            _serverSettings = serverSettings;
        }

        public BasePaymentResponse Post(LinkPayPalAccountRequest request)
        {
            return _payPalServiceFactory.GetInstance().LinkAccount(request.AccountId, request.AuthCode);
        }

        public BasePaymentResponse Post(UnlinkPayPalAccountRequest request)
        {
            return _payPalServiceFactory.GetInstance().UnlinkAccount(request.AccountId);
        }

        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardRequest request)
        {
            return _paymentServiceFactory.GetInstance().DeleteTokenizedCreditcard(request.CardToken);
        }

        public PairingResponse Post(PairingForPaymentRequest request)
        {
            var response =  _paymentServiceFactory.GetInstance().Pair(request.OrderId, request.CardToken, request.AutoTipPercentage, request.AutoTipAmount);
            if (response.IsSuccessful)
            {
                var order = _orderDao.FindById(request.OrderId);
                var ibsAccountId = _accountDao.GetIbsAccountId(order.AccountId, null);
                if (!UpdateOrderPaymentType(ibsAccountId.Value, order.IBSOrderId.Value))
                {
                    response.IsSuccessful = false;
                    _paymentServiceFactory.GetInstance().VoidPreAuthorization(request.OrderId);
                }
            }
            return response;
        }

        private bool UpdateOrderPaymentType(int ibsAccountId, int ibsOrderId, string companyKey = null)
        {
            return _ibsServiceProvider.Booking(companyKey).UpdateOrderPaymentType(ibsAccountId, ibsOrderId, _serverSettings.ServerData.IBS.PaymentTypeCardOnFileId);
        }

        public BasePaymentResponse Post(UnpairingForPaymentRequest request)
        {
            return _paymentServiceFactory.GetInstance().Unpair(request.OrderId);
        }
    }
}
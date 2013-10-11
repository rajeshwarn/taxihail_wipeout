using System;
using System.Linq;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class MailSender : IIntegrationEventHandler,
          IEventHandler<OrderCompleted>,
          IEventHandler<OrderStatusChanged>
    {
        readonly Func<BookingDbContext> _contextFactory;
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configurationManager;
        private readonly IPayPalExpressCheckoutPaymentDao _payPalExpressCheckoutPaymentDao;
        private readonly ICreditCardPaymentDao _cardPaymentDao;
        private readonly ICreditCardDao _creditCardDao;

        public MailSender(Func<BookingDbContext> contextFactory, 
            ICommandBus commandBus, 
            IConfigurationManager configurationManager,
            IPayPalExpressCheckoutPaymentDao payPalExpressCheckoutPaymentDao,
            ICreditCardPaymentDao cardPaymentDao,
            ICreditCardDao creditCardDao
            )
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _configurationManager = configurationManager;
            _payPalExpressCheckoutPaymentDao = payPalExpressCheckoutPaymentDao;
            _cardPaymentDao = cardPaymentDao;
            _creditCardDao = creditCardDao;
        }

        public void Handle(OrderCompleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderStatus = context.Find<OrderStatusDetail>(@event.SourceId);

                if (orderStatus != null)
                {
                     if (orderStatus.IBSStatusId == "wosDONE" && @event.Fare.GetValueOrDefault() > 0)
                     {
                         
                         var account = context.Find<AccountDetail>(orderStatus.AccountId);
                         var command = new SendReceipt
                         {
                             Id = Guid.NewGuid(),
                             OrderId = @event.SourceId,
                             EmailAddress = account.Email,
                             IBSOrderId = orderStatus.IBSOrderId.GetValueOrDefault(),
                             TransactionDate = orderStatus.PickupDate,
                             VehicleNumber = orderStatus.VehicleNumber,
                             Fare = @event.Fare.GetValueOrDefault(),
                             Toll = @event.Toll.GetValueOrDefault(),
                             Tip = @event.Tip.GetValueOrDefault(),
                             Tax = @event.Tax.GetValueOrDefault(),
                         };

#warning Copy and paste
                         var creditCardPayment = _cardPaymentDao.FindByOrderId(@event.SourceId);
                         if (creditCardPayment != null)
                         {
                             command.CardOnFileInfo = new Commands.SendReceipt.CardOnFile(
                                 creditCardPayment.Amount,
                                 creditCardPayment.TransactionId,
                                 "Credit Card");

                             var creditCard = _creditCardDao.FindByToken(creditCardPayment.CardToken);
                             if (creditCard != null)
                             {
                                 command.CardOnFileInfo.LastFour = creditCard.Last4Digits;
                                 command.CardOnFileInfo.Company = creditCard.CreditCardCompany;
                                 command.CardOnFileInfo.FriendlyName = creditCard.FriendlyName;
                             }
                         }

                         var paypalPayment = _payPalExpressCheckoutPaymentDao.FindByOrderId(@event.SourceId);
                         if (paypalPayment != null)
                         {
                             command.CardOnFileInfo = new Commands.SendReceipt.CardOnFile(
                                       paypalPayment.Amount,
                                        paypalPayment.TransactionId,
                                       "PayPal");
                         }


                         _commandBus.Send(command);
                     }
                }
            }
        }

        public void Handle(OrderStatusChanged @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var sendDriverAssignedMail = _configurationManager.GetSetting("Booking.DriverAssignedConfirmationEmail", false);
                if (sendDriverAssignedMail && @event.Status.IBSStatusId == "wosASSIGNED")
                {
                    var order = context.Find<OrderDetail>(@event.SourceId);
                    var orderStatus = context.Find<OrderStatusDetail>(@event.SourceId);
                    var account = context.Find<AccountDetail>(@event.Status.AccountId);

                    var command = new SendAssignedConfirmation
                    {
                        Id = Guid.NewGuid(),
                        EmailAddress = account.Email,
                        IBSOrderId = order.IBSOrderId.GetValueOrDefault(),
                        PickupDate = order.PickupDate,
                        Fare = order.EstimatedFare.GetValueOrDefault(),
                        PickupAddress = order.PickupAddress,
                        DropOffAddress = order.DropOffAddress,
                        Settings = new SendBookingConfirmationEmail.BookingSettings
                            {
                                Name = account.Name,
                                Phone = account.Phone,
                                ChargeType = order.Settings.ChargeType,
                                Passengers = order.Settings.Passengers,
                                VehicleType = order.Settings.VehicleType
                            },
                        TransactionDate = order.CreatedDate,
                        VehicleNumber = orderStatus.VehicleNumber
                    };

                    _commandBus.Send(command);
                }
            }
        }
    }
}

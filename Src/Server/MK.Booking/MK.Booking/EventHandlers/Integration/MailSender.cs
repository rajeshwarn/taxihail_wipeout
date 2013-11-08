using System;
using System.Linq;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
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
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>,
        IEventHandler<CreditCardPaymentCaptured>,
          IEventHandler<OrderStatusChanged>
    {
        readonly Func<BookingDbContext> _contextFactory;
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configurationManager;
        
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly ICreditCardDao _creditCardDao;

        public MailSender(Func<BookingDbContext> contextFactory, 
            ICommandBus commandBus, 
            IConfigurationManager configurationManager,            
            IOrderPaymentDao orderPaymentDao,
            ICreditCardDao creditCardDao
            )
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _configurationManager = configurationManager;            
            _orderPaymentDao = orderPaymentDao;
            _creditCardDao = creditCardDao;
        }

        public void Handle(CreditCardPaymentCaptured @event)
        {
            SendReceipt(@event.OrderId);            
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            SendReceipt(@event.OrderId);
        }

        private void SendReceipt(Guid orderId)
        {
            double? mdtFare;
            double? mdtTip;
            double? mdtToll;
            double? mdtTax;

            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(orderId);
                mdtFare = order.Fare;
                mdtTip = order.Tip;
                mdtToll = order.Toll;
                mdtTax = order.Tax;
            }

            SendReceipt(orderId, mdtFare, mdtTip, mdtToll, mdtTax);
        }


        public void Handle(OrderCompleted @event)
        {
                          
            SendReceipt(@event.SourceId, @event.Fare, @event.Tip, @event.Toll, @event.Tax);            
        }

        private void SendReceipt(Guid orderId, double? mdtFare, double? mdtTip, double? mdtToll, double? mdtTax)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderStatus = context.Find<OrderStatusDetail>(orderId);
                if (orderStatus != null)
                {
                    var orderPayment =
                        context.Set<OrderPaymentDetail>().SingleOrDefault(p => p.OrderId == orderStatus.OrderId);
                    if (orderStatus.IBSStatusId == "wosDONE" &&
                        (mdtFare.GetValueOrDefault() > 0 || ((orderPayment != null) && (orderPayment.Amount > 0))))
                    {
                        var paidFare = Convert.ToDouble((orderPayment != null) ? orderPayment.Meter : 0);
                        var fare = mdtFare.GetValueOrDefault() > 0 ? mdtFare.GetValueOrDefault() : paidFare;

                        var paidTip = Convert.ToDouble((orderPayment != null) ? orderPayment.Tip : 0);
                        var tip = mdtTip.GetValueOrDefault() > 0 ? mdtTip.GetValueOrDefault() : paidTip;

                        var account = context.Find<AccountDetail>(orderStatus.AccountId);
                        var command = new SendReceipt
                        {
                            Id = Guid.NewGuid(),
                            OrderId = orderId,
                            EmailAddress = account.Email,
                            IBSOrderId = orderStatus.IBSOrderId.GetValueOrDefault(),
                            TransactionDate = orderStatus.PickupDate,
                            VehicleNumber = orderStatus.VehicleNumber,
                            Fare = fare,
                            Toll = mdtToll.GetValueOrDefault(),
                            Tip = tip,
                            Tax = mdtTax.GetValueOrDefault(),
                        };

#warning Copy and paste
                        var orderPayement = _orderPaymentDao.FindByOrderId(orderId);
                        if (orderPayement != null)
                        {
                            command.CardOnFileInfo = new Commands.SendReceipt.CardOnFile(
                                orderPayement.Amount,
                                orderPayement.TransactionId,
                                orderPayement.AuthorizationCode,
                                orderPayement.Type == PaymentType.CreditCard
                                    ? "Credit Card"
                                    : orderPayement.Type.ToString());

                            if (orderPayement.CardToken.HasValue())
                            {
                                var creditCard = _creditCardDao.FindByToken(orderPayement.CardToken);
                                if (creditCard != null)
                                {
                                    command.CardOnFileInfo.LastFour = creditCard.Last4Digits;
                                    command.CardOnFileInfo.Company = creditCard.CreditCardCompany;
                                    command.CardOnFileInfo.FriendlyName = creditCard.FriendlyName;
                                }
                            }
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

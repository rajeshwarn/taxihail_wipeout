#region

using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Common.Entity;
using AutoMapper;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.CommandHandlers
{
    public class OrderCommandHandler :
        ICommandHandler<CreateOrder>,
        ICommandHandler<CancelOrder>,
        ICommandHandler<RemoveOrderFromHistory>,
        ICommandHandler<RateOrder>,
        ICommandHandler<ChangeOrderStatus>,
        ICommandHandler<PairForPayment>,
        ICommandHandler<UnpairForPayment>,
        ICommandHandler<NotifyOrderTimedOut>,
        ICommandHandler<ChangeOrderDispatchCompany>,
        ICommandHandler<SwitchOrderToNextDispatchCompany>,
        ICommandHandler<IgnoreDispatchCompanySwitch>
    {
        private readonly IEventSourcedRepository<Order> _repository;

        public OrderCommandHandler(IEventSourcedRepository<Order> repository)
        {
            _repository = repository;
        }

        public void Handle(CancelOrder command)
        {
            var order = _repository.Find(command.OrderId);
            order.Cancel();
            _repository.Save(order, command.Id.ToString());
        }
        
        public void Handle(ChangeOrderStatus command)
        {
            var order = _repository.Find(command.Status.OrderId);
            order.ChangeStatus(command.Status, command.Fare, command.Tip, command.Toll, command.Tax);

            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(NotifyOrderTimedOut command)
        {
            var order = _repository.Find(command.OrderId);
            order.NotifyOrderTimedOut();

            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(CreateOrder command)
        {
            var order = new Order(command.OrderId, command.AccountId, command.IBSOrderId, command.PickupDate,
                command.PickupAddress, command.DropOffAddress, command.Settings, command.EstimatedFare,
                command.UserAgent, command.ClientLanguageCode, command.UserLatitude, command.UserLongitude, command.UserNote, command.ClientVersion);

            if (command.Payment.PayWithCreditCard)
            {
                var payment = Mapper.Map<PaymentInformation>(command.Payment);
                order.SetPaymentInformation(payment);
            }
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(PairForPayment command)
        {
            var order = _repository.Find(command.OrderId);
            order.Pair(command.Medallion, command.DriverId, command.PairingToken, command.PairingCode,
                command.TokenOfCardToBeUsedForPayment, command.AutoTipAmount, command.AutoTipPercentage);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(RateOrder command)
        {
            var order = _repository.Find(command.OrderId);
            order.RateOrder(command.Note, command.RatingScores);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(RemoveOrderFromHistory command)
        {
            var order = _repository.Find(command.OrderId);
            order.RemoveFromHistory();
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(UnpairForPayment command)
        {
            var order = _repository.Find(command.OrderId);
            order.Unpair();
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(ChangeOrderDispatchCompany command)
        {
            var order = _repository.Find(command.OrderId);
            order.ChangeOrderDispatchCompany(command.DispatchCompanyName, command.DispatchCompanyKey);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(SwitchOrderToNextDispatchCompany command)
        {
            var order = _repository.Find(command.OrderId);
            order.SwitchOrderToNextDispatchCompany(command.IBSOrderId, command.CompanyKey, command.CompanyName);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(IgnoreDispatchCompanySwitch command)
        {
            var order = _repository.Find(command.OrderId);
            order.IgnoreDispatchCompanySwitch();
            _repository.Save(order, command.Id.ToString());
        }
    }
}
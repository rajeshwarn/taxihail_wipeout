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
        ICommandHandler<UnpairForPayment>
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
            order.ChangeStatus(command.Status);

            if (command.Status.Status == OrderStatus.Completed)
            {
                order.Complete(command.Fare, command.Tip, command.Toll, command.Tax);
            }
            else if (command.Fare > 0)
            {
                order.AddFareInformation(command.Fare, command.Tip, command.Toll, command.Tax);
            }

            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(CreateOrder command)
        {
            var order = new Order(command.OrderId, command.AccountId, command.IBSOrderId, command.PickupDate,
                command.PickupAddress, command.DropOffAddress, command.Settings, command.EstimatedFare,
                command.UserAgent, command.ClientLanguageCode, command.UserLatitude, command.UserLongitude, command.ClientVersion);

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
    }
}
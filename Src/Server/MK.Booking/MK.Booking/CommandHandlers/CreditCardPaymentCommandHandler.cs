using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class CreditCardPaymentCommandHandler :
        ICommandHandler<InitiateCreditCardPayment>,
        ICommandHandler<CaptureCreditCardPayment>
    {
        readonly IEventSourcedRepository<CreditCardPayment> _repository;

        public CreditCardPaymentCommandHandler(IEventSourcedRepository<CreditCardPayment> repository)
        {
            _repository = repository;
        }

        public void Handle(InitiateCreditCardPayment command)
        {
            var payment = new CreditCardPayment(command.PaymentId, command.OrderId, command.TransactionId, command.Amount,command.CardToken);
            _repository.Save(payment, command.Id.ToString());
        }

        public void Handle(CaptureCreditCardPayment command)
        {
            var payment = _repository.Get(command.PaymentId);
            payment.Capture();
            _repository.Save(payment, command.Id.ToString());
        }
    }
}
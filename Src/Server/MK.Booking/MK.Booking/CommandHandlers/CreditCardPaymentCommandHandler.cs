#region

using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.CommandHandlers
{
    public class CreditCardPaymentCommandHandler :
        ICommandHandler<InitiateCreditCardPayment>,
        ICommandHandler<CaptureCreditCardPayment>,
        ICommandHandler<LogCreditCardPaymentCancellationFailed>
    {
        private readonly IEventSourcedRepository<CreditCardPayment> _repository;

        public CreditCardPaymentCommandHandler(IEventSourcedRepository<CreditCardPayment> repository)
        {
            _repository = repository;
        }

        public void Handle(CaptureCreditCardPayment command)
        {
            var payment = _repository.Get(command.PaymentId);
            payment.Capture(command.Provider, command.AuthorizationCode);
            _repository.Save(payment, command.Id.ToString());
        }

        public void Handle(InitiateCreditCardPayment command)
        {
            var payment = new CreditCardPayment(command.PaymentId, command.OrderId, command.TransactionId,
                command.Amount, command.Meter, command.Tip, command.CardToken, command.Provider);
            _repository.Save(payment, command.Id.ToString());
        }

        public void Handle(LogCreditCardPaymentCancellationFailed command)
        {
            var payment = _repository.Get(command.PaymentId);
            payment.CancellationFailed(command.Reason);
            _repository.Save(payment, command.Id.ToString());
        }
    }
}
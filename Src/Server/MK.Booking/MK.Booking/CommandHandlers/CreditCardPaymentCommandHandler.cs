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
        ICommandHandler<LogCreditCardError>
    {
        private readonly IEventSourcedRepository<CreditCardPayment> _repository;

        public CreditCardPaymentCommandHandler(IEventSourcedRepository<CreditCardPayment> repository)
        {
            _repository = repository;
        }

        public void Handle(CaptureCreditCardPayment command)
        {
            var payment = _repository.Get(command.PaymentId);
            payment.Capture(command.Provider, command.TotalAmount, command.MeterAmount, command.TipAmount,
                command.TaxAmount, command.TollAmount, command.SurchargeAmount, command.AuthorizationCode, command.TransactionId,
                command.PromotionUsed, command.AmountSavedByPromotion, command.NewCardToken, 
                command.AccountId, command.IsSettlingOverduePayment, command.IsForPrepaidOrder, command.FeeType, command.BookingFees);
            _repository.Save(payment, command.Id.ToString());
        }

        public void Handle(InitiateCreditCardPayment command)
        {
            var payment = new CreditCardPayment(command.PaymentId,command.OrderId, command.TransactionId, command.Amount,
                command.Meter, command.Tip, command.CardToken, command.Provider, command.CompanyKey);
            _repository.Save(payment, command.Id.ToString());
        }

        public void Handle(LogCreditCardError command)
        {
            var payment = _repository.Get(command.PaymentId);
            payment.ErrorThrown(command.Reason, command.AccountId);
            _repository.Save(payment, command.Id.ToString());
        }
    }
}
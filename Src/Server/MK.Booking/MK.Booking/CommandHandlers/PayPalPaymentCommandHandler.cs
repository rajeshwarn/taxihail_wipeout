﻿#region

using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.CommandHandlers
{
    public class PayPalPaymentCommandHandler :
        ICommandHandler<InitiatePayPalExpressCheckoutPayment>,
        ICommandHandler<CancelPayPalExpressCheckoutPayment>,
        ICommandHandler<CompletePayPalExpressCheckoutPayment>,
        ICommandHandler<LogCancellationFailurePayPalPayment>

    {
        private readonly IEventSourcedRepository<PayPalPayment> _repository;

        public PayPalPaymentCommandHandler(IEventSourcedRepository<PayPalPayment> repository)
        {
            _repository = repository;
        }

        public void Handle(CancelPayPalExpressCheckoutPayment command)
        {
            var payment = _repository.Get(command.PaymentId);
            payment.Cancel();
            _repository.Save(payment, command.Id.ToString());
        }

        public void Handle(CompletePayPalExpressCheckoutPayment command)
        {
            var payment = _repository.Get(command.PaymentId);
            payment.Complete(command.TransactionId, command.PayPalPayerId);
            _repository.Save(payment, command.Id.ToString());
        }

        public void Handle(InitiatePayPalExpressCheckoutPayment command)
        {
            var payment = new PayPalPayment(command.PaymentId, command.OrderId, command.Token, command.Amount, command.Meter, command.Tip, command.Tax);
            _repository.Save(payment, command.Id.ToString());
        }

        public void Handle(LogCancellationFailurePayPalPayment command)
        {
            var payment = _repository.Get(command.PaymentId);
            payment.LogCancellationError(command.Reason);
            _repository.Save(payment, command.Id.ToString());
        }
    }
}
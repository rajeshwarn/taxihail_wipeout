using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class PayPalPaymentCommandHandler: 
        ICommandHandler<InitiatePayPalExpressCheckoutPayment>,
        ICommandHandler<CancelPayPalExpressCheckoutPayment>,
        ICommandHandler<CompletePayPalExpressCheckoutPayment>

    {
        private readonly IEventSourcedRepository<PayPalPayment> _repository;

        public PayPalPaymentCommandHandler(IEventSourcedRepository<PayPalPayment> repository)
        {
            _repository = repository;
        }

        public void Handle(InitiatePayPalExpressCheckoutPayment command)
        {
            var payment = new PayPalPayment(command.PaymentId, command.OrderId, command.Token, command.Amount, command.Meter, command.Tip );

            _repository.Save(payment, command.Id.ToString());
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
    }
}

using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdatePaymentProfile : ICommand
    {
        public UpdatePaymentProfile()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid? DefaultCreditCard { get; set; }
        public double? DefaultTipAmount { get; set; }
        public double? DefaultTipPercent { get; set; }
    }
}
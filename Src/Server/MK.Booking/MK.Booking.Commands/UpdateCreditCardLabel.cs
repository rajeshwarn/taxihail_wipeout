using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateCreditCardLabel : ICommand
    {
        public UpdateCreditCardLabel()
        {
            Id = Guid.NewGuid();
        }

        public Guid CreditCardId { get; set; }
        public Guid AccountId { get; set; }
        public Guid Id { get; set; }
        public string Label { get; set; }
    }
}

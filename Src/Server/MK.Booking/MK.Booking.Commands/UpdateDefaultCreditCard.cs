using Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateDefaultCreditCard : ICommand
    {
        public UpdateDefaultCreditCard()
        {
            Id = Guid.NewGuid();
        }

        public Guid CreditCardId { get; set; }
        public Guid AccountId { get; set; }
        public Guid Id { get; set; }
    }
}

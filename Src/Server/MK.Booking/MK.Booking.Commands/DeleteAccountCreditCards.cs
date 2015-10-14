using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DeleteAccountCreditCards : ICommand
    {
        public DeleteAccountCreditCards()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public Guid Id { get; private set; }

    }
}

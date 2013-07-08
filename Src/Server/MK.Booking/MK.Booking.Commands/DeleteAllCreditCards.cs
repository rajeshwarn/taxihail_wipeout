using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Events
{
    public class DeleteAllCreditCards : ICommand
    {
        public DeleteAllCreditCards()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }

        public Guid[] AccountIds { get; set; }
    }
}

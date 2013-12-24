#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class DeleteAllCreditCards : ICommand
    {
        public DeleteAllCreditCards()
        {
            Id = Guid.NewGuid();
        }

        public Guid[] AccountIds { get; set; }
        public Guid Id { get; private set; }
    }
}
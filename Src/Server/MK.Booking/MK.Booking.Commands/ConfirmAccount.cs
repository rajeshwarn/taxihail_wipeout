#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class ConfirmAccount : ICommand
    {
        public ConfirmAccount()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public string ConfimationToken { get; set; }
        public Guid Id { get; set; }
    }
}
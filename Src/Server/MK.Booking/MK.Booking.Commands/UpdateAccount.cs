#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class UpdateAccount : ICommand
    {
        public UpdateAccount()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Guid Id { get; set; }
    }
}
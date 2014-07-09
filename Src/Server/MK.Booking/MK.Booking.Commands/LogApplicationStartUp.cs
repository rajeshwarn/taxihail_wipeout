using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class LogApplicationStartUp : ICommand
    {
        public LogApplicationStartUp()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public Guid UserId { get; set; }
        public string Email { get; set; }
        public DateTime DateOccured { get; set; }
        public string ApplicationVersion { get; set; }
        public string Platform { get; set; }
        public string PlatformDetails { get; set; }
        public string ServerVersion { get; set; }
    }
}
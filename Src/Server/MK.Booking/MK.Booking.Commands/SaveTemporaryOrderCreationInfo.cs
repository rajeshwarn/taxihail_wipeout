using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class SaveTemporaryOrderCreationInfo : ICommand
    {
        public SaveTemporaryOrderCreationInfo()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }
        public string SerializedOrderCreationInfo { get; set; }

        public Guid Id { get; set; }
    }
}
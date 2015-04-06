using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateTripInfoInOrderForManualRideLinq : ICommand
    {
        public UpdateTripInfoInOrderForManualRideLinq()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public double? Distance { get; set; }
        public double? Fare { get; set; }
        public double? Tax { get; set; }
        public double? Tip { get; set; }
        public double? Toll { get; set; }
        public double? Extra { get; set; }
        public DriverInfos DriverInfo { get; set; }
        public DateTime? EndTime { get; set; }

        public string PairingToken { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Common.Entity
{
    public class ManualRideLinqDetails
    {
        public Guid OrderId { get; set; }
        public Guid AccountId { get; set; }
        // This is the code displayed on the taxi rig for the user to type
        public string PairingCode { get; set; }
        // This is the token to use to Get or Delete info.
        public string PairingToken { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsCancelled { get; set; }
        public double Distance { get; set; }
        public double StartingLongitude { get; set; }
        public double StartingLatitude { get; set; }

    }
}

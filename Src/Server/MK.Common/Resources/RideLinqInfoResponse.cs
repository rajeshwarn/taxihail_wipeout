using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Common.Resources
{
    public class RideLinqInfoResponse
    {
        public Guid OrderId { get; set; }

        public double? Distance { get; set; }

        public double? Total { get; set; }

        public double? Fare { get; set; }

        public double? Toll { get; set; }

        public double? Extra { get; set; }

        public double? Tip { get; set; }

        public double? Surcharge { get; set; }

        public double? Tax { get; set; }

        public string Medallion { get; set; }

        public int DriverId { get; set; }

        public string PairingCode { get; set; }
    }
}

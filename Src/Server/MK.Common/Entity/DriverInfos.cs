#region

using System.Linq;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Common.Entity
{
    public class DriverInfos
    {
        public string VehicleType { get; set; }

        public string VehicleMake { get; set; }

        public string VehicleModel { get; set; }

        public string VehicleColor { get; set; }

        public string VehicleRegistration { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName
        {
            get { return Params.Get(FirstName, LastName).Where(s => s.HasValue()).JoinBy(" "); }
        }

        public string MobilePhone { get; set; }
    }
}
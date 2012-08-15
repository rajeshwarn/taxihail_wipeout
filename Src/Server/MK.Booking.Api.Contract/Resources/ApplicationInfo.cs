using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class ApplicationInfo : BaseDTO
    {

        public string Version { get; set; }
        public string SiteName { get; set; }

    }
}

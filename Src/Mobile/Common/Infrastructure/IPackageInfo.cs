using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface IPackageInfo
    {
        string Platform
        {
            get;     
        }

        string Version { get; }
        string UserAgent { get; }
    }
}

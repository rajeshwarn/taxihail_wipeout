
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IApplicationInfoService
	{
        ApplicationInfo GetAppInfo( );

        void ClearAppInfo();

        void CheckVersion();

        
    }
}


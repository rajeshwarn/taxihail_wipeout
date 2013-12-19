using System;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IAppSettings
	{
        string ApplicationName {get;}		        
        string ServiceUrl { get; set; }
        
        bool ErrorLogEnabled{ get; }
		string ErrorLog{ get; }

        bool CanChangeServiceUrl { get; }                
        
		string DefaultServiceUrl{ get; }

        bool TwitterEnabled{ get; }       
        string TwitterConsumerKey{ get; }
        string TwitterCallback{ get; }
        string TwitterConsumerSecret{ get; }
        string TwitterRequestTokenUrl{ get; }
        string TwitterAccessTokenUrl{ get; }
        string TwitterAuthorizeUrl { get; }

        bool FacebookEnabled { get; }
        string FacebookAppId{ get; }
	}
}


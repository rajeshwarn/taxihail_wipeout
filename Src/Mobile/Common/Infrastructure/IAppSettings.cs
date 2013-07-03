using System;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IAppSettings
	{
        string ApplicationName {get;}

        int[] InvalidProviderIds { get; }
        string ServiceUrl { get; set; }

	    bool CanChooseProvider { get; }
        
        bool ErrorLogEnabled{ get; }
        bool CanChangeServiceUrl { get; }
        
        string ErrorLog{ get; }
        string SiteUrl{ get; }
        string PhoneNumber(int? providerId);
        string PhoneNumberDisplay(int? providerId);
        string DefaultServiceUrl{ get; }

        bool TutorialEnabled{ get; }        

        bool TwitterEnabled{ get; }       


        string TwitterConsumerKey{ get; }
        string TwitterCallback{ get; }
        string TwitterConsumerSecret{ get; }
        string TwitterRequestTokenUrl{ get; }
        string TwitterAccessTokenUrl{ get; }
        string TwitterAuthorizeUrl { get; }

        bool FacebookEnabled { get; }
        string FacebookAppId{ get; }

        string SupportEmail { get; }

        bool RatingEnabled { get; }

        bool StreetNumberScreenEnabled { get; }

        bool PayByCreditCardEnabled { get; }
        bool PushNotificationsEnabled { get; }

        bool HideNoPreference { get; }

		ClientPaymentSettings ClientPaymentSettings { get;}



	}
}


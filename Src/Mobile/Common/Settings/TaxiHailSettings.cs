using System;
using apcurium.MK.Common.Entity;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.Mobile.Settings
{
	public class TaxiHailSettings
    {
		public TaxiHailSettings()
		{
			//default values here
			ApplicationName = "Taxi Hail";	
			CanChangeServiceUrl = false;
		}

		[Display(Name = "Application Name", Description="Application name as displayed in message")]
		public string ApplicationName { get; private set; }

		[Display(Name = "Can Change Service Url", Description="Display a button on the login page to change the API server url")]
		public bool CanChangeServiceUrl { get; private set; }  

		public string ServiceUrl { get; set; }
		public bool ErrorLogEnabled{ get; private set; }
		public string DefaultServiceUrl{ get; private set; }
		public bool TwitterEnabled{ get; private set; }       
		public string TwitterConsumerKey{ get; private set; }
		public string TwitterCallback{ get; private set; }
		public string TwitterConsumerSecret{ get; private set; }
		public string TwitterRequestTokenUrl{ get; private set; }
		public string TwitterAccessTokenUrl{ get; private set; }
		public string TwitterAuthorizeUrl { get; private set; }
		public bool FacebookEnabled { get; private set; }
		public string FacebookAppId{ get; private set; }

		public bool HideNoPreference { get; private set; }
		public bool DestinationIsRequired { get; private set; }
		public bool HideCallDispactherButton { get; private set; }
		public DirectionSetting.TarifMode TarifMode { get; private set; }
		public double MaxFareEstimate { get; private set; }
		public bool DisableFutureBooking { get; private set; }
		public bool HideDestination { get; private set; }
		public bool StreetNumberScreenEnabled { get; private set; }
		public bool ShowTermAndConditions { get; private set; }
		public bool SendReceiptAvailable { get; private set; }
		public bool RatingEnabled { get; private set; }
		public bool ShowEstimateWarning { get; private set; }
		public bool ShowCallDriver { get; private set; }
		public bool ShowVehicleInformation { get; private set; }
		public bool TutorialEnabled { get; private set; }
		public bool HideReportProblem { get; private set; }

		public string PlacesTypes { get; private set; }
		public string PriceFormat { get; private set; }
		public string DistanceFormat { get; private set; }
		public string SearchFilter { get; private set; }
		public string DefaultRadius { get; private set; }
		public string MaxDistance { get; private set; }
		public string DefaultPhoneNumberDisplay { get; private set; }
		public string DefaultPhoneNumber { get; private set; }
		public string ShowEstimate { get; private set; }
		public string NumberOfCharInRefineAddress { get; private set; }
		public string AccountActivationEnabled { get; private set; }
		public string HideRebookOrder { get; private set; }
		public string ShowPassengerName { get; private set; }
		public string ShowPassengerPhone { get; private set; }
		public string ShowPassengerNumber { get; private set; }
		public string AboutUsUrl { get; private set; }
		public string ShowRingCodeField { get; private set; }
		public string ClientPollingInterval { get; private set; }
		public string HideCallDispatchButton { get; private set; }
		public string SupportEmail { get; private set; }
    }
}


using System;
using apcurium.MK.Common.Entity;
using System.ComponentModel.DataAnnotations;

namespace MK.Common.Configuration
{
	public class TaxiHailSetting
    {
		public TaxiHailSetting()
		{
			//default values here
			ApplicationName = "Taxi Hail";	
			ShowEstimateWarning = true;
            DefaultRadius = 500;
            AccountActivationDisabled = true;
            ShowVehicleInformation = true;
            ErrorLogFile = "errorlog.txt";
            ErrorLogEnabled = true;
            SupportEmail = "taxihail@apcurium.com";
            ShowPassengerName = true;
            ShowPassengerNumber = true;
            ShowPassengerPhone = true;
            ShowRingCodeField = true;
            TutorialEnabled = true;
		}

		[Display(Name = "Application Name", Description="Application name as displayed in message")]
		public string ApplicationName { get; private set; }
		[Display(Name = "Can Change Service Url", Description="Display a button on the login page to change the API server url")]
		public bool CanChangeServiceUrl { get; private set; }
        [Display(Name = "Service Url", Description="Url of the TaxiHail Server")]
		public string ServiceUrl { get; set; }
        [Display(Name = "Error Log Enabled", Description="Flag to enable the log of the errors in file")]
		public bool ErrorLogEnabled{ get; private set; }
        [Display(Name = "Error Log File", Description="Path of the error Log file")]
        public string ErrorLogFile{ get; private set; }


        [Display(Name = "Twitter Enabled", Description="Enable register/log in with Twitter")]
		public bool TwitterEnabled{ get; private set; }
        [Display(Name = "Twitter Consumer Key", Description="Twitter API settings")]
		public string TwitterConsumerKey{ get; private set; }
        [Display(Name = "Twitter CallBack", Description="Twitter API settings")]
		public string TwitterCallback{ get; private set; }
        [Display(Name = "Twitter Consumer Secret", Description="Twitter API settings")]
		public string TwitterConsumerSecret{ get; private set; }
        [Display(Name = "Twitter Token Url", Description="Twitter API settings")]
		public string TwitterRequestTokenUrl{ get; private set; }
        [Display(Name = "Twitter Access Token Url", Description="Twitter API settings")]
		public string TwitterAccessTokenUrl{ get; private set; }
        [Display(Name = "Twitter Authorize Url", Description="Twitter API settings")]
		public string TwitterAuthorizeUrl { get; private set; }
        [Display(Name = "Facebook Enabled", Description="Enable register/log in with Facebook")]
		public bool FacebookEnabled { get; private set; }
        [Display(Name = "Facebook App Id", Description="Facebook API settings")]
		public string FacebookAppId{ get; private set; }
        [Display(Name = "Account Activation Disabled", Description="Disable the confirmation requirement")]
        public bool AccountActivationDisabled { get; private set; }
        [Display(Name = "Show Terms and Conditions", Description="Display and require T&C screen")]
        public bool ShowTermsAndConditions { get; private set; }

        [Display(Name = "Tutorial Enabled", Description="Enable the tutorial")]
        public bool TutorialEnabled { get; private set; }
        [Display(Name = "Tutorial slides disabled", Description="Index of the slides to hide")]
        public string DisabledTutorialSlides { get; private set; }       
        [Display(Name = "Hide Report Problem", Description="Remove button in the menu")]
        public bool HideReportProblem { get; private set; }
        [Display(Name = "Default Phone Number", Description="Phone number as displayed to the user (1.800.XXX.XXXX)")]
        public string DefaultPhoneNumberDisplay { get; private set; }
        [Display(Name = "Default Phone Number", Description="Phone number as dialed")]
        public string DefaultPhoneNumber { get; private set; }
        [Display(Name = "About Us Url", Description="Url of the page on the company website")]
        public string AboutUsUrl { get; private set; }
        [Display(Name = "Support Email", Description="Email address to contact the company")]
        public string SupportEmail { get; private set; }

		[Display(Name = "Hide No Preference Option", Description="Settings to hide the no preference option in vehicule, company list etc.")]
		public bool HideNoPreference { get; private set; }
        [Display(Name = "Destination Required", Description="Flag to add destination as required")]
		public bool DestinationIsRequired { get; private set; }
        [Display(Name = "Disable Future Booking", Description="Hide button to scheduled a booking in the future")]
        public bool DisableFutureBooking { get; private set; }
        [Display(Name = "Hide Destination", Description="Hide destination address")]
        public bool HideDestination { get; private set; }
        [Display(Name = "Tarif Mode", Description="How the tarif calculation is done: by the app or by IBS")]
        public TarifMode TarifMode { get; private set; }
        [Display(Name = "Max Fare Estimate", Description="Maximum amount for estimation, if greater, a 'call the company' message is displayed")]
		public double MaxFareEstimate { get; private set; }
        [Display(Name = "Flate Rate", Description="Flate Rate for estimation")]
        public double FlateRate { get; private set; }
        [Display(Name = "Rate Per Km", Description="Rate per km for estimation")]
        public double RatePerKm { get; private set; }
        [Display(Name = "Street Number Screen Enabled", Description="Enable the change street number screen")]
		public bool StreetNumberScreenEnabled { get; private set; }
        [Display(Name = "Number of Char in Refine Address", Description="Number of characters in the textbox to change street number")]
        public int NumberOfCharInRefineAddress { get; private set; }
        [Display(Name = "Show Estimate Warning", Description="Show or not the the warning about the estimate being only an estimate")]
        public bool ShowEstimateWarning { get; private set; }
        [Display(Name = "Show Estimate", Description="Show an estimate")]
        public bool ShowEstimate { get; private set; }
        [Display(Name = "Use Theme Color On Map Icons", Description="Use company color for pickup and destination map icons")]
        public bool UseThemeColorForMapIcons { get; private set; }

        [Display(Name = "Show Passenger Name", Description="Show the passenger name field in confirmation")]
        public bool ShowPassengerName { get; private set; }
        [Display(Name = "Show Passenger Phone", Description="Show the passenger phone field in confirmation")]
        public bool ShowPassengerPhone { get; private set; }
        [Display(Name = "Show Passenger Number", Description="Show the passenger number field in confirmation")]
        public bool ShowPassengerNumber { get; private set; }
        [Display(Name = "Show Ring Code", Description="Show the ring code field in confirmation")]
        public bool ShowRingCodeField { get; private set; }

        [Display(Name = "Send Receipt Available", Description="Can the user send a receipt for the order")]
        public bool SendReceiptAvailable { get; private set; }
        [Display(Name = "Rating Enabled", Description="Can the user rate the order when it's done")]
		public bool RatingEnabled { get; private set; }		
        [Display(Name = "Show Call Driver", Description="Show button on the status screen to call the driver")]
		public bool ShowCallDriver { get; private set; }
        [Display(Name = "Show Vehicule Information", Description="Show vehicule informatino when available")]
		public bool ShowVehicleInformation { get; private set; }
        [Display(Name = "Client Polling Interval", Description="Status refresh interval")]
        public int ClientPollingInterval { get; private set; }
        [Display(Name = "Hide Call Dispatch Button", Description="Hide button to call dispatch in panel menu, status screens")]
        public bool HideCallDispatchButton { get; private set; }

		[Display(Name = "Place Types", Description="Give a list of Google Maps places types to filter search")]
		public string PlacesTypes { get; private set; }
        [Display(Name = "Place API Key", Description="Key for the Google Maps API")]
        public string PlacesApiKey { get; private set; }
        [Display(Name = "Price Format", Description="Format to display amount")]
		public string PriceFormat { get; private set; }
        [Display(Name = "Distance Format", Description="Format to display distance")]
		public string DistanceFormat { get; private set; }
        [Display(Name = "Search Filter", Description="Filter for geolocation search")]
		public string SearchFilter { get; private set; }
        [Display(Name = "Default Radius", Description="Default radius for places search")]
        public int DefaultRadius { get; private set; }
        [Display(Name = "Max Distance", Description="??")]
        public int MaxDistance { get; private set; }
        [Display(Name = "Default Latitude", Description="Default latitude to display the map before geoloc is done")]
        public double DefaultLatitude { get; private set; }
        [Display(Name = "Default Longitude", Description="Default longitude to display the map before geoloc is done")]
        public double DefaultLongitude { get; private set; }
        [Display(Name = "Range", Description="???")]
        public double Range { get; private set; }

        public bool AllowAddressRange { get; private set; }

        [Display(Name = "Need a Valid Tarif", Description="Prevent order when tarif is not available")]
        public bool NeedAValidTarif { get; private set; }
        [Display(Name = "Hide Rebook Order", Description="Hide the button in order history details")]
        public bool HideRebookOrder { get; private set; }
       
        [Display(Name = "SenderId", Description="Google Push Notification API Id")]
        public string SenderId { get; private set; }
        [Display(Name = "Push Notifications Enabled", Description="Enable push notificaiton for status changes")]
        public bool PushNotificationsEnabled { get; private set; }
        [Display(Name = "Hide Send Receipt", Description="???")]
        public bool HideSendReceipt { get; private set; }

    }
}


using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Entity;

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
            ErrorLogEnabled = true;
            SupportEmail = "taxihail@apcurium.com";
            ShowPassengerName = true;
            ShowPassengerNumber = true;
            ShowPassengerPhone = true;
            ShowRingCodeField = true;
		    ShowPassengerApartment = true;
            TutorialEnabled = true;
			HidePayNowButtonDuringRide = false;
			DefaultCardRequiredToPayNow = false;
		    CanSkipRatingRequired = true;
			ShowAssignedVehicleNumberOnPin = true;
			ZoomOnNearbyVehicles = false;
			ZoomOnNearbyVehiclesCount = 6;
			ZoomOnNearbyVehiclesRadius = 2400;

            CardIOToken = "af444ebbc4844f57999c52cc82d50478";
			CompanySettings = "Client.ShowEstimateWarning,Client.DestinationIsRequired,IBS.TimeDifference,IBS.PickupZoneToExclude,IBS.DestinationZoneToExclude,IBS.ValidateDestinationZone,IBS.ValidatePickupZone,Client.HideCallDispatchButton,Client.ShowAssignedVehicleNumberOnPin,Client.ZoomOnNearbyVehicles,Client.ZoomOnNearbyVehiclesCount,Client.ZoomOnNearbyVehiclesRadius,DefaultBookingSettings.ChargeTypeId,DefaultBookingSettings.NbPassenger,DefaultBookingSettings.ProviderId,DefaultBookingSettings.VehicleTypeId,Receipt.Note,Client.HideReportProblem,OrderStatus.ServerPollingInterval,IBS.NoteTemplate,AccountActivationDisabled,AvailableVehicles.Enabled,AvailableVehicles.Radius,AvailableVehicles.Count,Store.AppleLink,Store.PlayLink";
			DefaultTipPercentage = 15;
            DirectionDataProvider = MapProvider.Google;
			SMSConfirmationEnabled = false;
		    EtaPaddingRatio = 1;
		    DisableChargeTypeWhenCardOnFile = false;
		    VehicleTypeSelectionEnabled = false;
		    SendPushAsSMS = false;
            AllowSimultaneousAppOrders = false;
		}

		[Display(Name = "Application Name", Description="Application name as displayed in message")]
		public string ApplicationName { get; private set; }
		[Display(Name = "Can Change Service Url", Description="Display a button on the login page to change the API server url")]
		public bool CanChangeServiceUrl { get; private set; }
        [Display(Name = "Service Url", Description="Url of the TaxiHail Server")]
		public string ServiceUrl { get; set; }
        [Display(Name = "Error Log Enabled", Description="Flag to enable the log of the errors in file")]
		public bool ErrorLogEnabled{ get { return true; } private set{ } }
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
        
		[Display(Name = "Facebook Publish Enabled", Description="Facebook Publish Enabled")]
		public bool FacebookPublishEnabled { get; private set; }

		[Display(Name = "Facebook Enabled", Description="Enable register/log in with Facebook")]
		public bool FacebookEnabled { get; private set; }
        [Display(Name = "Facebook App Id", Description="Facebook API settings")]
		public string FacebookAppId{ get; private set; }



        [Display(Name = "Account Activation Disabled", Description="Disable the confirmation requirement")]
        public bool AccountActivationDisabled { get; private set; }
		[Display(Name = "Account Activation By SMS", Description="Enable the activation by SMS")]
        public bool SMSConfirmationEnabled { get; private set; }
        [Display(Name = "Disable Charge type when card on file", Description = "When active, locks the user on Card on File payment type if a credit card is registered")]
        public bool DisableChargeTypeWhenCardOnFile { get; private set; }
        [Display(Name = "Enable vehicle type selection", Description = "Hide the vehicle type selection box")]
        public bool VehicleTypeSelectionEnabled { get; private set; }

        [Display(Name = "SMS source number", Description = "Number from which the sms confirmation number will be sent")]
        public string SMSFromNumber { get; private set; }
        [Display(Name = "Twilio SMS account id", Description = "Account id for Twilio")]
        public string SMSAccountSid { get; private set; }
        [Display(Name = "Twilio SMS authentication token", Description = "Authentication token for twilio")]
        public string SMSAuthToken { get; private set; }
        [Display(Name = "Send push notifications as SMS", Description = "Send push notifications as SMS")]
        public bool SendPushAsSMS { get; private set; }
        [Display(Name = "Show Terms and Conditions", Description="Display and require T&C screen")]
        public bool ShowTermsAndConditions { get; private set; }
		[Display(Name = "Hide Mobile Knownledge and Apcurium logos", Description="In the menu")]
		public bool HideMkApcuriumLogos { get; private set; }
        
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
		[Display(Name = "Show Eta", Description="Show eta")]
		public bool ShowEta { get; private set; }
		[Display(Name = "Google Map Key", Description="Google API Key for business, required for directions aka eta feature")]
        public string GoogleMapKey { get; private set; }
        [Display(Name = "Eta Padding Ratio", Description = "Eta duration padding ratio (multiply duration in seconds by...)")]
		public double EtaPaddingRatio { get; private set; }
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
        [Display(Name = "Show Passenger Apartment", Description = "Show the passenger apartment field in confirmation")]
        public bool ShowPassengerApartment { get; private set; }
        [Display(Name = "Send Receipt Available", Description="Can the user send a receipt for the order")]
        public bool SendReceiptAvailable { get; private set; }
        [Display(Name = "Rating Enabled", Description="Can the user rate the order when it's done")]
		public bool RatingEnabled { get; private set; }		
		[Display(Name = "Rating mandatory", Description="remove the back button in rating screen and validate before leaving the screen")]
		public bool RatingRequired { get; private set; }
        [Display(Name = "User needs to rate before booking again", Description = "")]
        public bool CanSkipRatingRequired { get; private set; }

        [Display(Name = "Show Call Driver", Description="Show button on the status screen to call the driver")]
		public bool ShowCallDriver { get; private set; }
        [Display(Name = "Show Vehicule Information", Description="Show vehicule informatino when available")]
		public bool ShowVehicleInformation { get; private set; }
        [Display(Name = "Client Polling Interval", Description="Status refresh interval")]
        public int ClientPollingInterval { get; private set; }
        [Display(Name = "Hide Call Dispatch Button", Description="Hide button to call dispatch in panel menu, status screens")]
        public bool HideCallDispatchButton { get; private set; }
        [Display(Name = "Credit Card Is Mandatory", Description="If true, the user needs to have a card on file")]
        public bool CreditCardIsMandatory { get; private set; }
		[Display(Name = "Default Percentage Tip", Description="default value for the tip percentage ex: 15")]
		public int DefaultTipPercentage { get; private set; }
		[Display(Name = "Credit Card ChargeTypeId", Description = "ChargeTypeId of the Credit Card Charge Type")]
		public int? CreditCardChargeTypeId { get; private set; }


		[Display(Name = "Hide Pay Now Button During Ride", Description = "This will hide the pay now button, on the status screen, if the ride is not completed")]
		public bool HidePayNowButtonDuringRide { get; private set; }

		[Display(Name = "Default Card Required To Pay Now", Description = "This will hide the pay now button if the user doesn't have a default card setup.")]
		public bool DefaultCardRequiredToPayNow { get; private set; }


		[Display(Name = "Place Types", Description="Give a list of Google Maps places types to filter search")]
		public string PlacesTypes { get; private set; }
        [Display(Name = "Place Types", Description = "Give a list of Google Maps places types to filter search")]
        public string PlacesApiKey { get; private set; }
        [Display(Name = "Price Format Culture", Description="Format to display amount (Culture, ex: en-US)")]
		public string PriceFormat { get; private set; }
        [Display(Name = "Distance Format", Description="Format to display distance ('km' or 'mile')")]
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

        [Display(Name = "Foursquare Client Id", Description = "Foursquare API credentials Id")]
        public string FoursquareClientId { get; private set; }
        [Display(Name = "Foursquare Client Secret", Description = "Foursquare Client Secret")]
        public string FoursquareClientSecret { get; private set; }
        [Display(Name = "Foursquare Categories", Description = "filter venues for include only those categories")]
        public string FoursquarePlacesTypes { get; private set; }

        [Display(Name = "TomTom Map Toolkit API Key", Description = "TomTom Map Toolkit API Key")]
        public string TomTomMapToolkitKey { get; private set; }

        [Display(Name = "Direction Map Provider", Description = "Map Provider to use for Directions/Estimates")]
        public MapProvider DirectionDataProvider { get; private set; }

        public bool AllowAddressRange { get; private set; }

        [Display(Name = "Need a Valid Tarif", Description="Prevent order when tarif is not available")]
        public bool NeedAValidTarif { get; private set; }
        [Display(Name = "Hide Rebook Order", Description="Hide the button in order history details")]
        public bool HideRebookOrder { get; private set; }
       
        [Display(Name = "SenderId", Description="Google Push Notification API Id")]
        public string SenderId { get; private set; }
        [Display(Name = "Push Notifications Enabled", Description="Enable push notificaiton for status changes")]
        public bool PushNotificationsEnabled { get; private set; }
        [Display(Name = "Hide Send Receipt", Description="Hides the send receipt button in the app")]
        public bool HideSendReceipt { get; private set; }

        [Display(Name = "Card.IO Token", Description="Token for the Card.IO API (If empty, hides the button)")]
        public string CardIOToken { get; private set; }

        [Display(Name = "Company Settings", Description = "List of settings that can be modified by the taxi company")]
        public string CompanySettings { get; private set; }
					
		[Display(Name = "Lower Left Latitude", Description="Lower Left Latitude limit to be used when searching for an address")]
		public double? LowerLeftLatitude { get; private set; }
		[Display(Name = "Lower Left Longitude", Description="Lower Left Longitude limit to be used when searching for an address")]
		public double? LowerLeftLongitude { get; private set; }

		[Display(Name = "Upper Right Latitude", Description="Upper Right Latitude limit to be used when searching for an address")]
		public double? UpperRightLatitude { get; private set; }
		[Display(Name = "Upper Right Longitude", Description="Upper Right Longitude limit to be used when searching for an address")]
		public double? UpperRightLongitude { get; private set; }

		[Display(Name = "Zoom on nearby vehicles", Description="Enable zooming on nearby vehicles")]
		public bool ZoomOnNearbyVehicles { get; private set; }
		[Display(Name = "Zoom on nearby vehicles radius", Description="Inclusion radius in meters when zooming on nearby vehicles")]
		public int ZoomOnNearbyVehiclesRadius { get; private set; }
		[Display(Name = "Zoom on nearby vehicles count", Description="Maximum of included vehicles when zooming on nearby vehicles")]
		public int ZoomOnNearbyVehiclesCount { get; private set; }
        
		[Display(Name = "Show Assigned Vehicle Number On Pin", Description="Enable displaying the assigned vehicle number over its pin on the map")]
		public bool ShowAssignedVehicleNumberOnPin { get; private set; }

        [Display(Name = "VAT is Enabled", Description = "Enables the VAT calculation in the receipt")]
        public bool VATIsEnabled { get; private set; }

        [Display(Name = "VAT Percentage", Description = "Percentage to use to calculate the VAT portion of a fare")]
        public int VATPercentage { get; private set; }

        [Display(Name = "Allow Simultaneous Orders", Description = "Allow to have more than one active order")]
        public bool AllowSimultaneousAppOrders { get; private set; }
        
        [Display(Name = "Google AdWords Conversion Tracking ID", Description = "Conversion ID used for Google Conversion Tracking")]
        public string GoogleAdWordsConversionId { get; private set; }
        [Display(Name = "Google AdWords Conversion Tracking Label", Description = "Conversion Label used for Google Conversion Tracking")]
        public string GoogleAdWordsConversionLabel { get; private set; }
    }
}


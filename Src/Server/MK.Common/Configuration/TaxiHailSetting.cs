using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;

namespace MK.Common.Configuration
{
	public class TaxiHailSetting
    {
		public TaxiHailSetting()
		{
            TaxiHail = new TaxiHailSettingContainer();

		    OrderStatus = new OrderStatusSettingContainer
		    {
                ClientPollingInterval = 10
		    };

		    GCM = new GCMSettingContainer
		    {
		        SenderId = "385816297456"
		    };

		    Direction = new DirectionSettingContainer
		    {
		        TarifMode = TarifMode.AppTarif
		    };

		    NearbyPlacesService = new NearbyPlacesServiceSettingContainer
		    {
                DefaultRadius = 500
		    };

		    Map = new MapSettingContainer
		    {
		        PlacesApiKey = "AIzaSyAd-ezA2SeVTSNqsu6aMmAkdlP3UqEVPWE"
		    };

		    GeoLoc = new GeoLocSettingContainer
		    {
                PlacesTypes = "airport,amusement_park,art_gallery,bank,bar,bus_station,cafe,casino,clothing_store,convenience_store,courthouse,department_store,doctor,embassy,food,gas_station,hospital,lawyer,grocery_or_supermarket,library,liquor_store,lodging,local_government_office,movie_theater,moving_company,museum,night_club,park,parking,pharmacy,police,physiotherapist"
		    };

            AvailableVehicles = new AvailableVehiclesSettingContainer
            {
                Enabled = true,
                Count = 10,
                Radius = 2000
            };

            ShowEstimateWarning = true;
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
		    CanSkipRatingRequired = true;
			ShowAssignedVehicleNumberOnPin = true;
			ZoomOnNearbyVehicles = false;
			ZoomOnNearbyVehiclesCount = 6;
			ZoomOnNearbyVehiclesRadius = 2400;

            CardIOToken = "af444ebbc4844f57999c52cc82d50478";
			
			DefaultTipPercentage = 15;
            DirectionDataProvider = MapProvider.Google;
			SMSConfirmationEnabled = false;
		    EtaPaddingRatio = 1;
		    DisableChargeTypeWhenCardOnFile = false;
		    VehicleTypeSelectionEnabled = false;
		    SendPushAsSMS = false;
            AllowSimultaneousAppOrders = false;

		    MaxFareEstimate = 100;
		}

        public TaxiHailSettingContainer TaxiHail { get; protected set; }
        public OrderStatusSettingContainer OrderStatus { get; protected set; }
        public GCMSettingContainer GCM { get; protected set; }
        public DirectionSettingContainer Direction { get; protected set; }
        public NearbyPlacesServiceSettingContainer NearbyPlacesService { get; protected set; }
        public MapSettingContainer Map { get; protected set; }
        public GeoLocSettingContainer GeoLoc { get; protected set; }
        public AvailableVehiclesSettingContainer AvailableVehicles { get; protected set; }

		[Display(Name = "Can Change Service Url", Description="Display a button on the login page to change the API server url")]
		public bool CanChangeServiceUrl { get; protected set; }
        [Display(Name = "Service Url", Description="Url of the TaxiHail Server")]
		public string ServiceUrl { get; set; }
        
        [Display(Name = "Error Log Enabled", Description="Flag to enable the log of the errors in file")]
		public bool ErrorLogEnabled{ get { return true; } protected set{ } }
        
        [Display(Name = "Twitter Enabled", Description="Enable register/log in with Twitter")]
		public bool TwitterEnabled{ get; protected set; }
        [Display(Name = "Twitter Consumer Key", Description="Twitter API settings")]
		public string TwitterConsumerKey{ get; protected set; }
        [Display(Name = "Twitter CallBack", Description="Twitter API settings")]
		public string TwitterCallback{ get; protected set; }
        [Display(Name = "Twitter Consumer Secret", Description="Twitter API settings")]
		public string TwitterConsumerSecret{ get; protected set; }
        [Display(Name = "Twitter Token Url", Description="Twitter API settings")]
		public string TwitterRequestTokenUrl{ get; protected set; }
        [Display(Name = "Twitter Access Token Url", Description="Twitter API settings")]
		public string TwitterAccessTokenUrl{ get; protected set; }
        [Display(Name = "Twitter Authorize Url", Description="Twitter API settings")]
		public string TwitterAuthorizeUrl { get; protected set; }
        
		[Display(Name = "Facebook Publish Enabled", Description="Facebook Publish Enabled")]
		public bool FacebookPublishEnabled { get; protected set; }
        [Display(Name = "Facebook Enabled", Description="Enable register/log in with Facebook")]
		public bool FacebookEnabled { get; protected set; }
        [Display(Name = "Facebook App Id", Description="Facebook API settings")]
		public string FacebookAppId{ get; protected set; }

        [Display(Name = "Account Activation Disabled", Description="Disable the confirmation requirement")]
        public bool AccountActivationDisabled { get; protected set; }
		[Display(Name = "Account Activation By SMS", Description="Enable the activation by SMS")]
        public bool SMSConfirmationEnabled { get; protected set; }
        
        [Display(Name = "Disable Charge type when card on file", Description = "When active, locks the user on Card on File payment type if a credit card is registered")]
        public bool DisableChargeTypeWhenCardOnFile { get; protected set; }
        
        [Display(Name = "Enable vehicle type selection", Description = "Hide the vehicle type selection box")]
        public bool VehicleTypeSelectionEnabled { get; protected set; }

        [Display(Name = "SMS source number", Description = "Number from which the sms confirmation number will be sent")]
        public string SMSFromNumber { get; protected set; }
        [Display(Name = "Twilio SMS account id", Description = "Account id for Twilio")]
        public string SMSAccountSid { get; protected set; }
        [Display(Name = "Twilio SMS authentication token", Description = "Authentication token for twilio")]
        public string SMSAuthToken { get; protected set; }
        [Display(Name = "Send push notifications as SMS", Description = "Send push notifications as SMS")]
        public bool SendPushAsSMS { get; protected set; }
        [Display(Name = "Show Terms and Conditions", Description="Display and require T&C screen")]
        public bool ShowTermsAndConditions { get; protected set; }
		[Display(Name = "Hide Mobile Knownledge and Apcurium logos", Description="In the menu")]
		public bool HideMkApcuriumLogos { get; protected set; }
        
        [Display(Name = "Tutorial Enabled", Description="Enable the tutorial")]
        public bool TutorialEnabled { get; protected set; }
        
		[Display(Name = "Tutorial slides disabled", Description="Index of the slides to hide")]
        public string DisabledTutorialSlides { get; protected set; }       
        [Display(Name = "Hide Report Problem", Description="Remove button in the menu")]
        public bool HideReportProblem { get; protected set; }
        [Display(Name = "Default Phone Number", Description="Phone number as displayed to the user (1.800.XXX.XXXX)")]
        public string DefaultPhoneNumberDisplay { get; protected set; }
        [Display(Name = "Default Phone Number", Description="Phone number as dialed")]
        public string DefaultPhoneNumber { get; protected set; }
        [Display(Name = "About Us Url", Description="Url of the page on the company website")]
        public string AboutUsUrl { get; protected set; }
        [Display(Name = "Support Email", Description="Email address to contact the company")]
        public string SupportEmail { get; protected set; }

		[Display(Name = "Hide No Preference Option", Description="Settings to hide the no preference option in vehicule, company list etc.")]
		public bool HideNoPreference { get; protected set; }
        [Display(Name = "Destination Required", Description="Flag to add destination as required")]
		public bool DestinationIsRequired { get; protected set; }
        [Display(Name = "Disable Future Booking", Description="Hide button to scheduled a booking in the future")]
        public bool DisableFutureBooking { get; protected set; }
        [Display(Name = "Hide Destination", Description="Hide destination address")]
        public bool HideDestination { get; protected set; }
        
        [Display(Name = "Max Fare Estimate", Description="Maximum amount for estimation, if greater, a 'call the company' message is displayed")]
		public double MaxFareEstimate { get; protected set; }
        
        
        [Display(Name = "Street Number Screen Enabled", Description="Enable the change street number screen")]
		public bool StreetNumberScreenEnabled { get; protected set; }
        [Display(Name = "Number of Char in Refine Address", Description="Number of characters in the textbox to change street number")]
        public int NumberOfCharInRefineAddress { get; protected set; }
        [Display(Name = "Show Estimate Warning", Description="Show or not the the warning about the estimate being only an estimate")]
        public bool ShowEstimateWarning { get; protected set; }
        [Display(Name = "Show Estimate", Description="Show an estimate")]
        public bool ShowEstimate { get; protected set; }
		[Display(Name = "Show Eta", Description="Show eta")]
		public bool ShowEta { get; protected set; }
		[Display(Name = "Google Map Key", Description="Google API Key for business, required for directions aka eta feature")]
        public string GoogleMapKey { get; protected set; }
        [Display(Name = "Eta Padding Ratio", Description = "Eta duration padding ratio (multiply duration in seconds by...)")]
		public double EtaPaddingRatio { get; protected set; }
        [Display(Name = "Use Theme Color On Map Icons", Description="Use company color for pickup and destination map icons")]
        public bool UseThemeColorForMapIcons { get; protected set; }

        [Display(Name = "Show Passenger Name", Description="Show the passenger name field in confirmation")]
        public bool ShowPassengerName { get; protected set; }
        [Display(Name = "Show Passenger Phone", Description="Show the passenger phone field in confirmation")]
        public bool ShowPassengerPhone { get; protected set; }
        [Display(Name = "Show Passenger Number", Description="Show the passenger number field in confirmation")]
        public bool ShowPassengerNumber { get; protected set; }
        [Display(Name = "Show Ring Code", Description="Show the ring code field in confirmation")]
        public bool ShowRingCodeField { get; protected set; }
        [Display(Name = "Show Passenger Apartment", Description = "Show the passenger apartment field in confirmation")]
        public bool ShowPassengerApartment { get; protected set; }
        [Display(Name = "Send Receipt Available", Description="Can the user send a receipt for the order")]
        public bool SendReceiptAvailable { get; protected set; }
        [Display(Name = "Rating Enabled", Description="Can the user rate the order when it's done")]
		public bool RatingEnabled { get; protected set; }		
		[Display(Name = "Rating mandatory", Description="remove the back button in rating screen and validate before leaving the screen")]
		public bool RatingRequired { get; protected set; }
        [Display(Name = "User needs to rate before booking again", Description = "")]
        public bool CanSkipRatingRequired { get; protected set; }

        [Display(Name = "Show Call Driver", Description="Show button on the status screen to call the driver")]
		public bool ShowCallDriver { get; protected set; }
        [Display(Name = "Show Vehicule Information", Description="Show vehicule informatino when available")]
		public bool ShowVehicleInformation { get; protected set; }
        
        [Display(Name = "Hide Call Dispatch Button", Description="Hide button to call dispatch in panel menu, status screens")]
        public bool HideCallDispatchButton { get; protected set; }
        [Display(Name = "Credit Card Is Mandatory", Description="If true, the user needs to have a card on file")]
        public bool CreditCardIsMandatory { get; protected set; }
		[Display(Name = "Default Percentage Tip", Description="default value for the tip percentage ex: 15")]
		public int DefaultTipPercentage { get; protected set; }


		[Display(Name = "Hide Pay Now Button During Ride", Description = "This will hide the pay now button, on the status screen, if the ride is not completed")]
		public bool HidePayNowButtonDuringRide { get; protected set; }
        
        [Display(Name = "Price Format Culture", Description="Format to display amount (Culture, ex: en-US)")]
		public string PriceFormat { get; protected set; }
        [Display(Name = "Distance Format", Description="Format to display distance ('km' or 'mile')")]
		public DistanceFormat DistanceFormat { get; protected set; }
        
        [Display(Name = "Range", Description="???")]
        public double Range { get; protected set; }

        [Display(Name = "Foursquare Client Id", Description = "Foursquare API credentials Id")]
        public string FoursquareClientId { get; protected set; }
        [Display(Name = "Foursquare Client Secret", Description = "Foursquare Client Secret")]
        public string FoursquareClientSecret { get; protected set; }
        [Display(Name = "Foursquare Categories", Description = "filter venues for include only those categories")]
        public string FoursquarePlacesTypes { get; protected set; }

        [Display(Name = "TomTom Map Toolkit API Key", Description = "TomTom Map Toolkit API Key")]
        public string TomTomMapToolkitKey { get; protected set; }

        [Display(Name = "Direction Map Provider", Description = "Map Provider to use for Directions/Estimates")]
        public MapProvider DirectionDataProvider { get; protected set; }

        public bool AllowAddressRange { get; protected set; }

        
        [Display(Name = "Hide Rebook Order", Description="Hide the button in order history details")]
        public bool HideRebookOrder { get; protected set; }
       
        
        [Display(Name = "Push Notifications Enabled", Description="Enable push notificaiton for status changes")]
        public bool PushNotificationsEnabled { get; protected set; }
        [Display(Name = "Hide Send Receipt", Description="Hides the send receipt button in the app")]
        public bool HideSendReceipt { get; protected set; }

        [Display(Name = "Card.IO Token", Description="Token for the Card.IO API (If empty, hides the button)")]
        public string CardIOToken { get; protected set; }

        
					
		[Display(Name = "Lower Left Latitude", Description="Lower Left Latitude limit to be used when searching for an address")]
		public double? LowerLeftLatitude { get; protected set; }
		[Display(Name = "Lower Left Longitude", Description="Lower Left Longitude limit to be used when searching for an address")]
		public double? LowerLeftLongitude { get; protected set; }

		[Display(Name = "Upper Right Latitude", Description="Upper Right Latitude limit to be used when searching for an address")]
		public double? UpperRightLatitude { get; protected set; }
		[Display(Name = "Upper Right Longitude", Description="Upper Right Longitude limit to be used when searching for an address")]
		public double? UpperRightLongitude { get; protected set; }

		[Display(Name = "Zoom on nearby vehicles", Description="Enable zooming on nearby vehicles")]
		public bool ZoomOnNearbyVehicles { get; protected set; }
		[Display(Name = "Zoom on nearby vehicles radius", Description="Inclusion radius in meters when zooming on nearby vehicles")]
		public int ZoomOnNearbyVehiclesRadius { get; protected set; }
		[Display(Name = "Zoom on nearby vehicles count", Description="Maximum of included vehicles when zooming on nearby vehicles")]
		public int ZoomOnNearbyVehiclesCount { get; protected set; }
        
		[Display(Name = "Show Assigned Vehicle Number On Pin", Description="Enable displaying the assigned vehicle number over its pin on the map")]
		public bool ShowAssignedVehicleNumberOnPin { get; protected set; }

        [Display(Name = "VAT is Enabled", Description = "Enables the VAT calculation in the receipt")]
        public bool VATIsEnabled { get; protected set; }

        [Display(Name = "VAT Percentage", Description = "Percentage to use to calculate the VAT portion of a fare")]
        public int VATPercentage { get; protected set; }

        [Display(Name = "Allow Simultaneous Orders", Description = "Allow to have more than one active order")]
        public bool AllowSimultaneousAppOrders { get; protected set; }
        
        [Display(Name = "Google AdWords Conversion Tracking ID", Description = "Conversion ID used for Google Conversion Tracking")]
        public string GoogleAdWordsConversionId { get; protected set; }
        [Display(Name = "Google AdWords Conversion Tracking Label", Description = "Conversion Label used for Google Conversion Tracking")]
        public string GoogleAdWordsConversionLabel { get; protected set; }
    }
}


using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Attributes;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Cryptography;
using System;

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
                PlacesTypes = "airport,amusement_park,art_gallery,bank,bar,bus_station,cafe,casino,clothing_store,convenience_store,courthouse,department_store,doctor,embassy,food,gas_station,hospital,lawyer,grocery_or_supermarket,library,liquor_store,lodging,local_government_office,movie_theater,moving_company,museum,night_club,park,parking,pharmacy,police,physiotherapist",
                SearchRadius = 45000
		    };

            AvailableVehicles = new AvailableVehiclesSettingContainer
            {
                Enabled = true,
                Count = 10,
                Radius = 2000
            };

			ShowEstimate = true;
		    Network = new NetworkSettingContainer
		    {
                Enabled = false
		    };

			Store = new StoreSettingContainer
			{
				AppleLink = "http://www.mobile-knowledge.com/",
                PlayLink = "http://www.mobile-knowledge.com/"
			};

            ShowEstimateWarning = true;
            AccountActivationDisabled = true;
            ShowVehicleInformation = true;
            ShowOrientedPins = false;
            IsDriverBonusEnabled = false;
            ChangeDropOffAddressMidTrip = false;
            ChangeCreditCardMidtrip = false;

#if DEBUG
            SupportEmail = "taxihail@apcurium.com";
            DebugViewEnabled = true;
#endif

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
            HideTHNetworkAppMenu = true;
            ShowOrderNumber = true;

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
            MapBoxKey = "pk.eyJ1IjoiZGV2dG9ueSIsImEiOiJjaWZ5OGJ0NXc0eWtxdXBrcXl2czF1eGY5In0.6qUEJWLnvqZ0_0Q6Xh2Gaw";

            AvailableVehicleRefreshRate = 5;

		    TwitterAccessTokenUrl = "https://api.twitter.com/oauth/access_token";
            TwitterAuthorizeUrl = "https://api.twitter.com/oauth/authorize";
            TwitterCallback = "http://www.taxihail.com/oauth";
            TwitterRequestTokenUrl = "https://api.twitter.com/oauth/request_token";
            
            InitialZoomLevel = 14;

            MaxNumberOfCardsOnFile = 1;
            SendZipCodeWhenTokenizingCard = false;

            FlightStats = new FlightStatsSettingsContainer
			{
				UseAirportDetails = false
			};
		}

		[Hidden]
		public bool AppleTestAccountUsed { get; set; }

        public TaxiHailSettingContainer TaxiHail { get; protected set; }
        public OrderStatusSettingContainer OrderStatus { get; protected set; }
        public GCMSettingContainer GCM { get; protected set; }
        public DirectionSettingContainer Direction { get; protected set; }
        public NearbyPlacesServiceSettingContainer NearbyPlacesService { get; protected set; }
        public MapSettingContainer Map { get; protected set; }
        public GeoLocSettingContainer GeoLoc { get; protected set; }
        public AvailableVehiclesSettingContainer AvailableVehicles { get; protected set; }
        public NetworkSettingContainer Network { get; protected set; }
		public FlightStatsSettingsContainer FlightStats { get; set; }
		public StoreSettingContainer Store { get; protected set; }

        [RequiredAtStartup]
		[Display(Name = "Configuration - Can Change Service Url", Description="Display a button on the login page to change the API server url")]
		public bool CanChangeServiceUrl { get; protected set; }

        [Hidden]
        [RequiredAtStartup]
        [Display(Name = "Service Url", Description="Url of the TaxiHail Server")]
		public string ServiceUrl { get; set; }

		[PropertyEncrypt]
        [SendToClient, CustomizableByCompany]
        [Display(Name = "Search - CraftyClicks Api Key", Description = "Enables the UK postcode address lookup using the CraftyClicks Api")]
        public string CraftyClicksApiKey { get; set; }

        [RequiredAtStartup, SendToClient, CustomizableByCompany]
        [Display(Name = "Twitter - Enabled", Description="Enable register/log in with Twitter")]
		public bool TwitterEnabled { get; protected set; }

		[PropertyEncrypt]
        [RequiredAtStartup, SendToClient, CustomizableByCompany]
        [Display(Name = "Twitter - Consumer Key", Description="Twitter API Consumer Key")]
		public string TwitterConsumerKey{ get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Use single button for now and later booking", Description = "Replace book now and book later buttons with a single button that will display both options in a popup. (This feature does not work with manual CMT ridelinq pairing.)")]
        public bool UseSingleButtonForNowAndLaterBooking { get; set; }

        [SendToClient]
        [Display(Name = "Available Vehicle - Local Mode", Description = "Available Vehicles provider in local market")]
        public LocalAvailableVehiclesModes LocalAvailableVehiclesMode { get; protected set; }

        [SendToClient]
        [Display(Name = "Available Vehicle - External Mode", Description = "Available Vehicles provider in external market")]
        public ExternalAvailableVehiclesModes ExternalAvailableVehiclesMode { get; protected set; }

		[PropertyEncrypt]
        [RequiredAtStartup, SendToClient, CustomizableByCompany]
        [Display(Name = "Twitter - Consumer Secret", Description = "Twitter API Consumer Secret")]
        public string TwitterConsumerSecret { get; protected set; }

        [SendToClient]
        [Display(Name = "Twitter - CallBack", Description="Twitter API Callback URL")]
		public string TwitterCallback{ get; protected set; }

		[PropertyEncrypt]
        [SendToClient]
        [Display(Name = "Twitter - Token Url", Description="Twitter API Token URL")]
		public string TwitterRequestTokenUrl{ get; protected set; }

		[PropertyEncrypt]
        [SendToClient]
        [Display(Name = "Twitter - Access Token Url", Description="Twitter API Access Token URL")]
		public string TwitterAccessTokenUrl{ get; protected set; }

		[PropertyEncrypt]
        [SendToClient]
        [Display(Name = "Twitter - Authorize Url", Description="Twitter API Authorize URL")]
		public string TwitterAuthorizeUrl { get; protected set; }

        [RequiredAtStartup, SendToClient, CustomizableByCompany]
        [Display(Name = "Facebook - Publish Enabled", Description="Facebook Publish Enabled")]
		public bool FacebookPublishEnabled { get; protected set; }

        [RequiredAtStartup, SendToClient, CustomizableByCompany]
        [Display(Name = "Facebook - Enabled", Description="Enable register/log in with Facebook")]
		public bool FacebookEnabled { get; protected set; }

		[PropertyEncrypt]
        [RequiredAtStartup, SendToClient, CustomizableByCompany]
        [Display(Name = "Facebook - App Id", Description="Facebook API settings")]
		public string FacebookAppId{ get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Account - Activation Disabled", Description="Disable the confirmation requirement")]
        public bool AccountActivationDisabled { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Available Vehicle - Refresh Rate", Description = "Modify the refresh delay (in seconds) of the available vehicles on the map.")]
	    public int AvailableVehicleRefreshRate { get; set; }

        [SendToClient, CustomizableByCompany]
		[Display(Name = "Account - Enable Activation By SMS", Description="Enable the activation by SMS")]
        public bool SMSConfirmationEnabled { get; protected set; }

        [SendToClient, CustomizableByCompany, RequiresTaxiHailPro]
        [Display(Name = "Configuration - Disable Charge type when card on file", Description = "When active, locks the user on Card on File payment type if a credit card is registered")]
        public bool DisableChargeTypeWhenCardOnFile { get; protected set; }

        [SendToClient, CustomizableByCompany, RequiresTaxiHailPro]
        [Display(Name = "Configuration - Enable vehicle type selection", Description = "Hide the vehicle type selection box")]
        public bool VehicleTypeSelectionEnabled { get; protected set; }

        [Display(Name = "SMS - Source Phone Number", Description = "Number from which the sms confirmation number will be sent")]
        public string SMSFromNumber { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show individual taxi marker only", Description = "When this setting is enabled, we will only show individual taxi markers (and won't replace with a cluster icon).")]
	    public bool ShowIndividualTaxiMarkerOnly { get; protected set; }

		[PropertyEncrypt]
        [Display(Name = "SMS -Twilio SMS account id", Description = "Account id for Twilio")]
        public string SMSAccountSid { get; protected set; }

		[PropertyEncrypt]
        [Display(Name = "SMS -Twilio SMS authentication token", Description = "Authentication token for twilio")]
        public string SMSAuthToken { get; protected set; }

        [CustomizableByCompany]
        [Display(Name = "SMS - Send push notifications as SMS", Description = "Send push notifications as SMS")]
        public bool SendPushAsSMS { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show Terms and Conditions", Description="Show and require T&C screen")]
        public bool ShowTermsAndConditions { get; protected set; }

        [SendToClient]
		[Display(Name = "Display - Hide Apcurium & MK logos", Description="Hide the MK and Apcurium logos in app menu")]
		public bool HideMkApcuriumLogos { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Tutorial Enabled", Description="Enable the tutorial")]
        public bool TutorialEnabled { get; protected set; }

        [SendToClient, CustomizableByCompany]
		[Display(Name = "Configuration - Tutorial slides disabled", Description="Index of the slides to hide (7 slides in total) comma separated")]
        public string DisabledTutorialSlides { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Hide Report Problem", Description="Hide Report Problem in app menu")]
        public bool HideReportProblem { get; protected set; }

        [SendToClient]
        [Display(Name = "Configuration - Default Phone Number", Description="Phone number as dialed")]
        public string DefaultPhoneNumber { get; protected set; }

        [SendToClient]
        [Display(Name = "Configuration - Default Phone Number (Display)", Description="Phone number as displayed to the user (1.800.XXX.XXXX)")]
        public string DefaultPhoneNumberDisplay { get; protected set; }

        [SendToClient]
        [Display(Name = "Configuration - Default Phone Number For Luxury", Description="Phone number as dialed")]
        public string DefaultPhoneNumberForLuxury { get; protected set; }

        [SendToClient]
        [Display(Name = "Configuration - Default Phone Number For Luxury (Display)", Description="Phone number as displayed to the user (1.800.XXX.XXXX)")]
        public string DefaultPhoneNumberForLuxuryDisplay { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Search - Enable airport filter button", Description = "Enables the use of the airport search filter button in the app.")]
        public bool IsAirportButtonEnabled { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Search - Enable train station filter button", Description = "Enables the use of the trains station search filter button in the app.")]
        public bool IsTrainStationButtonEnabled { get; set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - About Us Url", Description="Url of the page on the company website")]
        public string AboutUsUrl { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Email Setting - Support Email", Description="Email address to contact the company support")]
        public string SupportEmail { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Destination Required", Description="Flag to add destination as required")]
		public bool DestinationIsRequired { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Disable Immediate Booking", Description = "Hide button to immediate booking ('book now button')")]
        public bool DisableImmediateBooking { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Disable Future Booking", Description="Hide button to scheduled a booking in the future")]
        public bool DisableFutureBooking { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Hide Destination Button", Description="Hide destination button that allows user to enter a dropoff address")]
        public bool HideDestination { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Max Fare Estimate", Description="Maximum amount for estimation, if greater, a 'call the company' message is displayed")]
		public double MaxFareEstimate { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show Estimate Warning", Description="Show or not the the warning about the estimate being only an estimate")]
        public bool ShowEstimateWarning { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show Estimate", Description="Show a fare estimate")]
        public bool ShowEstimate { get; protected set; }

        [SendToClient, CustomizableByCompany, RequiresTaxiHailPro]
		[Display(Name = "Display - Show Eta", Description="Show nearest taxi ETA")]
		public bool ShowEta { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Eta Padding Ratio", Description = "Eta duration padding ratio (multiply duration in seconds by...)")]
		public double EtaPaddingRatio { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Use Theme Color On Map Icons", Description="Use company color for pickup and destination map icons")]
        public bool UseThemeColorForMapIcons { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show Passenger Name", Description="Show the passenger name field in confirmation")]
        public bool ShowPassengerName { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show Passenger Phone", Description="Show the passenger phone field in confirmation")]
        public bool ShowPassengerPhone { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show Passenger Number", Description="Show the passenger number field in confirmation")]
        public bool ShowPassengerNumber { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show Ring Code", Description="Show the ring code field in confirmation")]
        public bool ShowRingCodeField { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show Passenger Apartment", Description = "Show the passenger apartment field in confirmation")]
        public bool ShowPassengerApartment { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Send Receipt Available", Description="Enable the user to send a copy of the receipt from history")]
        public bool SendReceiptAvailable { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Rating - Trip Rating Enabled", Description="The user can rate the trip at the end of it")]
		public bool RatingEnabled { get; protected set; }

        [SendToClient]
		[Display(Name = "Rating - Trip Rating mandatory", Description="If YES, remove the back button on rating screen and message to rate displayed")]
		public bool RatingRequired { get; protected set; }

        [SendToClient]
        [Display(Name = "Rating - Can Skip Required Trip Rating", Description = "If NO, User MUST rate a ride before creating a new order")]
        public bool CanSkipRatingRequired { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show Call Driver", Description="Show button on the status screen to call the driver")]
		public bool ShowCallDriver { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show Vehicle Information", Description="Show vehicle information when available")]
		public bool ShowVehicleInformation { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Hide Call Dispatch Button", Description="Hide button to call dispatch in panel menu, status screens")]
        public bool HideCallDispatchButton { get; protected set; }

        [Obsolete("Use PaymentSetting 'CreditCardIsMandatory' instead", false)]
        [SendToClient, CustomizableByCompany, RequiresTaxiHailPro]
        [Display(Name = "Payment - Payment Method Mandatory", Description="If true, the user needs to have a payment method associated to his account (ie: Card on File or Paypal)")]
        public bool CreditCardIsMandatory { get; protected set; }

        [SendToClient, CustomizableByCompany, RequiresTaxiHailPro]
		[Display(Name = "Configuration - Default Percentage Tip", Description="default value for the tip percentage ex: 15")]
		public int DefaultTipPercentage { get; protected set; }

        [SendToClient]
		[Display(Name = "Display - Hide Pay Now Button During Ride", Description = "Hide the pay now button, on the status screen, if the ride is not completed")]
		public bool HidePayNowButtonDuringRide { get; protected set; }

        [SendToClient]
        [Display(Name = "Configuration - Price Format Culture", Description="Format to display amount (Culture, ex: en-US)")]
		public string PriceFormat { get; protected set; }

        [SendToClient]
        [Display(Name = "Configuration - Distance Format", Description="Format to display distance ('Km' or 'Mile')")]
		public DistanceFormat DistanceFormat { get; protected set; }

		[PropertyEncrypt]
        [SendToClient, CustomizableByCompany]
        [Display(Name = "Search - Foursquare Client Id", Description = "Foursquare API credentials Id")]
        public string FoursquareClientId { get; protected set; }

		[PropertyEncrypt]
        [SendToClient, CustomizableByCompany]
        [Display(Name = "Search - Foursquare Client Secret", Description = "Foursquare Client Secret")]
        public string FoursquareClientSecret { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Search - Foursquare Categories", Description = "Foursquare categories to include in search")]
        public string FoursquarePlacesTypes { get; protected set; }

		[PropertyEncrypt]
        [SendToClient, CustomizableByCompany]
        [Display(Name = "Map - TomTom Map Toolkit API Key", Description = "TomTom Map Toolkit API Key")]
        public string TomTomMapToolkitKey { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Map - Direction Map Provider", Description = "Map Provider to use for Directions/Estimates")]
        public MapProvider DirectionDataProvider { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Hide Rebook Order", Description="Hide Rebook button in app history view")]
        public bool HideRebookOrder { get; protected set; }

		[PropertyEncrypt]
        [SendToClient]
        [Display(Name = "Card IO Token", Description="Token for the Card.IO API (If empty, hides the button)")]
        public string CardIOToken { get; protected set; }

        [SendToClient, CustomizableByCompany]
		[Display(Name = "Search - Lower Left Latitude", Description="Lower Left Latitude limit to be used when searching for an address")]
        public double? LowerLeftLatitude { get; protected set; }

        [SendToClient, CustomizableByCompany]
		[Display(Name = "Search - Lower Left Longitude", Description="Lower Left Longitude limit to be used when searching for an address")]
        public double? LowerLeftLongitude { get; protected set; }

        [SendToClient, CustomizableByCompany]
		[Display(Name = "Search - Upper Right Latitude", Description="Upper Right Latitude limit to be used when searching for an address")]
        public double? UpperRightLatitude { get; protected set; }

        [SendToClient, CustomizableByCompany]
		[Display(Name = "Search - Upper Right Longitude", Description="Upper Right Longitude limit to be used when searching for an address")]
        public double? UpperRightLongitude { get; protected set; }

        [SendToClient, CustomizableByCompany]
		[Display(Name = "Map - Zoom on nearby vehicles", Description="Enable zooming on nearby vehicles")]
		public bool ZoomOnNearbyVehicles { get; protected set; }

        [SendToClient, CustomizableByCompany]
		[Display(Name = "Map - Zoom on nearby vehicles radius", Description="Inclusion radius in meters when zooming on nearby vehicles")]
		public int ZoomOnNearbyVehiclesRadius { get; protected set; }

        [SendToClient, CustomizableByCompany]
		[Display(Name = "Map - Zoom on nearby vehicles count", Description="Maximum of included vehicles when zooming on nearby vehicles")]
		public int ZoomOnNearbyVehiclesCount { get; protected set; }

        [SendToClient, CustomizableByCompany]
		[Display(Name = "Display - Show Assigned Vehicle Number On Pin", Description="Show the assigned vehicle number over its pin on the map")]
		public bool ShowAssignedVehicleNumberOnPin { get; protected set; }

        [Display(Name = "Configuration - VAT is Enabled", Description = "Enables the VAT calculation in the receipt")]
        public bool VATIsEnabled { get; protected set; }

        [Display(Name = "Configuration - VAT Percentage", Description = "Percentage to use to calculate the VAT portion of a fare")]
        public int VATPercentage { get; protected set; }

        [Display(Name = "Configuration - Allow Simultaneous Orders", Description = "Allow to have more than one active order")]
        public bool AllowSimultaneousAppOrders { get; protected set; }

		[PropertyEncrypt]
        [SendToClient]
        [Display(Name = "Configuration - Google AdWords Conversion Tracking ID", Description = "Conversion ID used for Google Conversion Tracking")]
        public string GoogleAdWordsConversionId { get; protected set; }

		[PropertyEncrypt]
        [SendToClient]
        [Display(Name = "Configuration - Google AdWords Conversion Tracking Label", Description = "Conversion Label used for Google Conversion Tracking")]
        public string GoogleAdWordsConversionLabel { get; protected set; }

		[PropertyEncrypt]
        [SendToClient]
        [Display(Name = "Configuration - Google Analytics Tracking ID", Description = "Company's Tracking ID used for Google Analytics")]
        public string GoogleAnalyticsTrackingId { get; protected set; }

	    [SendToClient, CustomizableByCompany]
        [Display(Name = "Map - Initial Zoom Level", Description = "Initial map zoom level (1 = Whole world, 14 = Default)")]
        public int InitialZoomLevel { get; set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Disable Automatic Zoom On Location", Description = "If enable, keep current zoom level instead of default zoom level when selceting a new location")]
        public bool DisableAutomaticZoomOnLocation { get; set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Warn for Fees on Cancel", Description = "Before cancelling an order, the user will be warned that he could be charged cancellation fees.")]
        public bool WarnForFeesOnCancel { get; set; }

        [SendToClient]
        [Display(Name = "Configuration - Promotion enabled", Description = "Enables promotion on the client and on the admin portal")]
        public bool PromotionEnabled { get; set; }

        [SendToClient]
        [Display(Name = "Account - Registration PayBack", Description = "Defines if the PayBack field when creating a new account is required or not")]
        public bool? IsPayBackRegistrationFieldRequired { get; set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show Message Driver", Description = "Show button on the status screen to text message the driver")]
        public bool ShowMessageDriver { get; set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Maximum number of Cards On File", Description = "Maximum number of Credit cards the client car add to his account")]
        public int MaxNumberOfCardsOnFile { get; set; }
        
        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Use vehicle direction", Description = "Available only with GEO. When enabled, the marked will be oriented according to the vehicle direction information")]
        public bool ShowOrientedPins { get; protected set; }
        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - ZipCode required when tokenizing card", Description = "Send the zip code when tokenizing card")]
        public bool SendZipCodeWhenTokenizingCard { get; set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Hide TaxiHail Network from menu", Description = "Hide THNetwork from app menu item")]
        public bool HideTHNetworkAppMenu { get; protected set; }

		[Obsolete("IsDriverBonusEnabled is now a market settings, configurable in the Customer Portal")]
        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Enable Driver Bonus", Description = "Offering a guaranteed bonus to drivers to boost the odds of getting a taxi.")]
        public bool IsDriverBonusEnabled { get; protected set; }

        [CustomizableByCompany]
        [Display(Name = "Display - Job Offer Prompt mesage to driver", Description = "Message that will prompt on driver console on the Accept/Decline screen")]
        public string MessagePromptedToDriver { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Enable Change Destination MidTrip", Description = "Possibility to add/change/remove the destination while in trip")]
        public bool ChangeDropOffAddressMidTrip { get; protected set; }
        
	    [SendToClient, CustomizableByCompany]
            [Display(Name = "Rating - Enable App Rating", Description = "User can be prompted to rate the app when on board a taxi")]
	    public bool EnableApplicationRating { get; protected set; }

	    [SendToClient, CustomizableByCompany]
	    [Display(Name = "Rating - Minimum Trips For App Rating", Description = "Minimum successful trips to allow user to rate applicatio")]
	    public int MinimumTripsForAppRating { get; protected set; }

	    [SendToClient, CustomizableByCompany]
	    [Display(Name = "Rating - Minimum Ride Rating Score for App Rating", Description = "Minimum ride rating score to allow user to rate application")]
	    public int MinimumRideRatingScoreForAppRating { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Display - Show Order Number", Description = "Show order number")]
        public bool ShowOrderNumber { get; protected set; }

		[CustomizableByCompany]
		[Display(Name = "Configuration - Display Extra Info in Receipt", Description = "Display extra info in receipt: vehicle info, vehicle registration, driver photo")]
		public bool ShowExtraInfoInReceipt { get; protected set; }

		[PropertyEncrypt]
        [SendToClient, CustomizableByCompany]
        [Display(Name = "Map - MapBox Key", Description = "BlackBerry MapBox Key")]
        public string MapBoxKey { get; protected set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Configuration - Change credit card while in trip", Description = "Allow the user to change his credit card while in trip")]
        public bool ChangeCreditCardMidtrip { get; protected set; }

        [CustomizableByCompany]
        [Display(Name = "Configuration - Unload Timeout", Description = "Time (in seconds) waiting for Charge Amounts from Driver")]
        public double ChargeAmountsTimeOut { get; set; }

        [PropertyEncrypt]
        [SendToClient]
        [Display(Name = "Configuration - Enable Debug View", Description = "Allows to view debug information by tapping on the version label in the menu")]
        public bool DebugViewEnabled { get; protected set; }
    }
}

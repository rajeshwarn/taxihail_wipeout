using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Attributes;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Enumeration.TimeZone;

namespace MK.Common.Configuration
{
    public class ServerTaxiHailSetting : TaxiHailSetting
    {
        public ServerTaxiHailSetting()
        {
            OrderStatus = new OrderStatusSettingContainer
            {
                ClientPollingInterval = 10,
                ServerPollingInterval = 10
            };

            GCM = new GCMSettingContainer
            {
                SenderId = "385816297456",
                APIKey = "AIzaSyC7eWqKEHj58xo3Tsuji4EH6HA7dn0T9bY"
            };

            Smtp = new SmtpSettingContainer
            {
                Host = "smtpcorp.com",
                Port = 2525,
                EnableSsl = false,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new CredentialsContainer
                {
                    Username = "Password01",
                    Password = "TaxiHail"
                }
            };

            APNS = new APNSSettingContainer();

            DefaultBookingSettings = new DefaultBookingSettingsSettingContainer
            {
                NbPassenger = 1,
                ChargeTypeId = ChargeTypes.PaymentInCar.Id
            };

            Store = new StoreSettingContainer
            {
                AppleLink = "http://www.mobile-knowledge.com/",
                PlayLink = "http://www.mobile-knowledge.com/"
            };

            IBS = new IBSSettingContainer
            {
                AutoDispatch = true,
                DefaultAccountPassword = "password",
                ExcludedVehicleTypeId = "22;21",
                NoteTemplate = "{{userNote}}\\r\\n{{buildingName}}",
                OrderPriority = true,
                TimeDifference = 0,
                ValidatePickupZone = true,
                RestApiUrl = @"http://cabmatedemo.drivelinq.com:8889/", // TODO: Set proper values
                RestApiUser = @"EUGENE",
                RestApiSecret = @"T!?_asF",
                WebServicesPassword = "test",
                WebServicesUserName = "taxi"
            };

            Email = new EmailSettingContainer
            {
                NoReply = "dotnotreply@taxihail.com"
            };

            Receipt = new ReceiptSettingContainer
            {
                Note = "Thank You!"
            };

            CustomerPortal = new CustomerPortalSettingContainer
            {
                Url = "http://customer.taxihail.com/api/",
                UserName = "taxihail@apcurium.com",
                Password = "apcurium5200!"
            };

            Network = new NetworkSettingContainer
            {
                PrimaryOrderTimeout = 60 * 5,   // 5 mins
                SecondaryOrderTimeout = 60 * 2, // 2 mins
                Enabled = false
            };

            HoneyBadger = new HoneyBadgerSettingContainer
            {
                ServiceUrl = "http://insight.cmtapi.com:8081/v1.1/availability?availState=1"
            };

            PayPalConversionRate = 1;
            SendDetailedPaymentInfoToDriver = true;
            CompanyTimeZone = TimeZones.NotSet;

            CmtGeo = new CmtGeoSettingContainer()
            {
                AppKey = "A47275341E57CB7C593DE3EDD5FCA",
                ServiceUrl = "http://geo-sandbox.cmtapi.com"
            };

            Gds = new CmtGdsSettingContainer()
            {
                ServiceUrl = "http://gds1.staging.aws.cmt.local:8180"
            };

        }

        public SmtpSettingContainer Smtp { get; protected set; }
        public APNSSettingContainer APNS { get; protected set; }
        public DefaultBookingSettingsSettingContainer DefaultBookingSettings { get; protected set; }
        public StoreSettingContainer Store { get; protected set; }
        public IBSSettingContainer IBS { get; protected set; }
        public EmailSettingContainer Email { get; protected set; }
        public ReceiptSettingContainer Receipt { get; protected set; }
        public CustomerPortalSettingContainer CustomerPortal { get; protected set; }
        public NetworkSettingContainer Network { get; protected set; }
        public HoneyBadgerSettingContainer HoneyBadger { get; protected set; }
        public CmtGeoSettingContainer CmtGeo { get; protected set; }
        public CmtGdsSettingContainer Gds { get; protected set; }

        [Display(Name = "Website - Hide Web signup button", Description = "Hide Sign Up button on web site")]
        public bool IsWebSignupHidden { get; protected set; }

        [Display(Name = "Payment - PayPal Region Info Override", Description = "Secret Paypal Setting (See Mathieu S.)")]
        public string PayPalRegionInfoOverride { get; protected set; }

        [Display(Name = "Payment - PayPal Conversion Rate", Description = "Paypal conversion rate to US dollar")]
        public decimal PayPalConversionRate { get; protected set; }

        [Display(Name = "Configuration - Hide fare info when Pay in Car", Description = "Hide fare information in receipt when user choose to pay in car.")]
        public bool HideFareInfoInReceipt { get; protected set; }

        [CustomizableByCompany]
        [Display(Name = "Configuration - Minimum Required App Version", Description = "Minimum required app version to create an order.")]
        public string MinimumRequiredAppVersion { get; private set; }

        [CustomizableByCompany, RequiresTaxiHailPro]
        [Display(Name = "Configuration - Send Payment Detail To Driver", Description = "Inform the driver of auto payment success or failure")]
        public bool SendDetailedPaymentInfoToDriver { get; private set; }

        [Display(Name = "Configuration - Disable Newer Version Popup", Description = "Disables the popup on the application telling the user that a new version is available")]
        public bool DisableNewerVersionPopup { get; private set; }

	    [Display(Name = "Configuration - Base Url Override", Description = "Overrides the base url of the application (ex: In account confirmation email)")]
        public string BaseUrl { get; private set; }

        [Display(Name = "Configuration - TaxiHail Pro", Description = "Company has access to TaxiHail Pro features")]
        public bool IsTaxiHailPro { get; protected set; }

        [CustomizableByCompany]
        [Display(Name = "Configuration - Driver Eta Notification Mode", Description = "Configures the notification mode to the driver for it's estimated time of arrival to the pickup location.")]
        public DriverEtaNotificationModes DriverEtaNotificationMode { get; protected set; }

        [Hidden]
        [Display(Name = "Configuration - Available to Admin (Hidden)", Description = "Comma delimited list of settings that are available to admins")]
        public string SettingsAvailableToAdmin { get; private set; }

        [Hidden]
        [Display(Name = "Configuration - Target", Description = "Deployment target server")]
        public DeploymentTargets Target { get; set; }

        [Display(Name = "Website - Display Social Media Links", Description = "Displays the Social Media links on the website")]
        public bool IsWebSocialMediaVisible { get; protected set; }

        [CustomizableByCompany]
        [Display(Name = "Website - Social Media URL (Website)", Description = "Adds the link to the Facebook address. If empty, no button will appear. Requires: Website - Display Social Media link")]
        public string SocialMediaFacebookURL { get; protected set; }

        [CustomizableByCompany]
        [Display(Name = "Website - Social Media URL (Website)", Description = "Adds the link to the Twitter address. If empty, no button will appear. Requires: Website - Display Social Media link")]
        public string SocialMediaTwitterURL { get; protected set; }

        [CustomizableByCompany]
        [Display(Name = "Website - Google+ URL (Website)", Description = "Adds the link to the Google+ address. If empty, no button will appear. Requires: Website - Display Social Media link")]
        public string SocialMediaGoogleURL { get; protected set; }

        [CustomizableByCompany]
        [Display(Name = "Website - Pinterest URL (Website)", Description = "Adds the link to the Pinterest address. If empty, no button will appear. Requires: Website - Display Social Media link")]
        public string SocialMediaPinterestURL { get; protected set; }

        [CustomizableByCompany]
        [Display(Name = "Configuration - Validate Admin Rules in Other Markets", Description = "Use the market booking rules defined by this company to validate orders in other markets")]
        public bool ValidateAdminRulesForExternalMarket { get; protected set; }

        [CustomizableByCompany]
        [Display(Name = "Configuration - Use Pairing Code When RideLinq Payment", Description = "If enable, will wait for Pairing Code from IBS before processing Cmt Payment")]
        public bool UsePairingCodeWhenUsingRideLinqCmtPayment { get; protected set; }        
        
        [CustomizableByCompany]
        [Display(Name = "Configuration - Company's time zone", Description = "Used to properly show dates in the correct time zone")]
        public TimeZones CompanyTimeZone { get; protected set; }

        [CustomizableByCompany]
        [Display(Name = "Configuration - Cancellation Fees Window", Description = "Window (in seconds) where the user can cancel an order without being charged cancellation fees. Window starts when taxi gets assigned.")]
        public int CancellationFeesWindow { get; protected set; }
        
        [CustomizableByCompany]
        [Display(Name = "Configuration - Hide Fare Estimate From IBS", Description = "Prevent sending fare estimate to IBS when creating an order. DO NOT enable this setting if the fare estimate is the real/flat ride fare.")]
        public bool HideFareEstimateFromIBS { get; protected set; }
    }
}
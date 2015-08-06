using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using apcurium.MK.Common.Configuration.Attributes;
using apcurium.MK.Common.Entity;
using MK.Common.Configuration;

namespace apcurium.MK.Common.Configuration
{
    public class TaxiHailSettingContainer
    {
        [RequiredAtStartup, SendToClient, CustomizableByCompany]
        [Display(Name = "Application Name", Description = "Application name as displayed in message")]
        public string ApplicationName { get; protected internal set; }

		[RequiredAtStartup, SendToClient]
		[Display(Name = "ApplicationKey", Description = "ApplicationKey")]
        public string ApplicationKey { get; protected internal set; }

		[RequiredAtStartup]
        public string AccentColor { get; protected internal set; }

        public string EmailFontColor { get; protected internal set; }

        public string SiteName { get; protected internal set; }
    }

    public class OrderStatusSettingContainer
    {
        [SendToClient, CustomizableByCompany]
        [Display(Name = "Client Polling Interval", Description = "Status refresh interval")]
        public int ClientPollingInterval { get; protected internal set; }

        [CustomizableByCompany]
        public int ServerPollingInterval { get; protected internal set; }
    }

    public class GCMSettingContainer
    {
        [SendToClient]
        [Display(Name = "GCM SenderId", Description = "Google Push Notification API Id")]
        public string SenderId { get; protected internal set; }

        [Display(Name = "GCM APIKey", Description = "GCM APIKey")]
        public string APIKey { get; protected internal set; }

        [Display(Name = "GCM PackageName", Description = "GCM PackageName")]
        public string PackageName { get; protected internal set; }
    }

    public class DirectionSettingContainer
    {
        [SendToClient]
        [Display(Name = "Tarif Mode", Description = "How the tarif calculation is done: by the app or by IBS")]
        public TarifMode TarifMode { get; protected internal set; }

        [Display(Name = "Need a Valid Tarif", Description = "Prevent order when tarif is not available")]
        public bool NeedAValidTarif { get; protected internal set; }

        [Display(Name = "Flate Rate", Description = "Flate Rate for estimation")]
        public decimal FlateRate { get; protected internal set; }

        [Display(Name = "Rate Per Km", Description = "Rate per km for estimation")]
        public double RatePerKm { get; protected internal set; }
    }

    public class NearbyPlacesServiceSettingContainer
    {
        [SendToClient]
        [Display(Name = "Default Radius", Description = "Default radius for places search")]
        public int DefaultRadius { get; protected internal set; }
    }

    public class MapSettingContainer
    {
        [SendToClient]
        [Display(Name = "Places Api Key", Description = "Google Places Api Key")]
        public string PlacesApiKey { get; protected internal set; }
    }

    public class GeoLocSettingContainer
    {
        [RequiredAtStartup, SendToClient, CustomizableByCompany]
        [Display(Name = "Default Latitude", Description = "Default latitude to display the map before geoloc is done")]
        public double DefaultLatitude { get; protected internal set; }

        [RequiredAtStartup, SendToClient, CustomizableByCompany]
        [Display(Name = "Default Longitude", Description = "Default longitude to display the map before geoloc is done")]
        public double DefaultLongitude { get; protected internal set; }

        [SendToClient, CustomizableByCompany]
        [Display(Name = "Place Types", Description = "Give a list of Google Maps places types to filter search")]
        public string PlacesTypes { get; protected internal set; }

        [RequiredAtStartup, SendToClient, CustomizableByCompany]
        [Display(Name = "Search Filter", Description = "Filter for geolocation search")]
        public string SearchFilter { get; protected internal set; }
    }

    public class AvailableVehiclesSettingContainer
    {
        [SendToClient, CustomizableByCompany]
        [Display(Name = "Enable Available Vehicles", Description = "Available Vehicles feature is enabled")]
        public bool Enabled { get; protected internal set; }

        [CustomizableByCompany]
        [Display(Name = "Max Available Vehicles Count", Description = "Maximum number of available vehicles to be shown")]
        public int Count { get; protected internal set; }

        [CustomizableByCompany]
        [Display(Name = "Available Vehicles Radius", Description = "Only available vehicle inside the radius will be taken into account")]
        public int Radius { get; protected internal set; }
    }

    public class SmtpSettingContainer
    {
        public SmtpSettingContainer()
        {
            Credentials = new CredentialsContainer();
        }

        [Display(Name = "SMTP Host", Description = "SMTP Host")]
        public string Host { get; protected internal set; }

        [Display(Name = "SMTP Port", Description = "SMTP Port")]
        public int Port { get; protected internal set; }

        [Display(Name = "SMTP UseDefaultCredentials", Description = "SMTP UseDefaultCredentials")]
        public bool UseDefaultCredentials { get; protected internal set; }

        [Display(Name = "SMTP EnableSSl", Description = "SMTP EnableSSl")]
        public bool EnableSsl { get; protected internal set; }

        [Display(Name = "SMTP DeliveryMethod", Description = "SMTP DeliveryMethod")]
        public SmtpDeliveryMethod DeliveryMethod { get; protected internal set; }

        public CredentialsContainer Credentials { get; protected internal set; }
    }

    public class CredentialsContainer
    {
        [Display(Name = "SMTP Username", Description = "SMTP Username")]
        public string Username { get; protected internal set; }

        [Display(Name = "SMTP Password", Description = "SMTP Password")]
        public string Password { get; protected internal set; }
    }

    public class APNSSettingContainer
    {
        [Display(Name = "APNS CertificatePassword", Description = "APNS CertificatePassword")]
        public string CertificatePassword { get; protected internal set; }

        [Display(Name = "APNS DevelopmentCertificatePath", Description = "APNS DevelopmentCertificatePath")]
        public string DevelopmentCertificatePath { get; protected internal set; }

        [Display(Name = "APNS ProductionCertificatePath", Description = "APNS ProductionCertificatePath")]
        public string ProductionCertificatePath { get; protected internal set; }
    }

    public class DefaultBookingSettingsSettingContainer
    {
        [CustomizableByCompany]
        public int? ChargeTypeId { get; protected internal set; }

        [CustomizableByCompany]
        public int NbPassenger { get; protected internal set; }

        [CustomizableByCompany]
        public int? ProviderId { get; protected internal set; }

        [CustomizableByCompany]
        public int? VehicleTypeId { get; protected internal set; }
    }

    public class StoreSettingContainer
    {
        [CustomizableByCompany]
        public string AppleLink { get; protected internal set; }

        [CustomizableByCompany]
        public string PlayLink { get; protected internal set; }
    }

    public class IBSSettingContainer
    {
        [Display(Name = "IBS AutoDispatch", Description = "IBS AutoDispatch")]
        public bool AutoDispatch { get; protected internal set; }

        [Display(Name = "IBS DefaultAccountPassword", Description = "IBS DefaultAccountPassword")]
        public string DefaultAccountPassword { get; protected internal set; }

        [Display(Name = "IBS TimeDifference", Description = "IBS TimeDifference")]
        [CustomizableByCompany]
        public long TimeDifference { get; set; }

        [Display(Name = "IBS FakeOrderStatusUpdate", Description = "IBS FakeOrderStatusUpdate")]
        public bool FakeOrderStatusUpdate { get; protected internal set; }

        [CustomizableByCompany]
        [Display(Name = "Driver Note Template", Description = "Driver Note Template")]
        public string NoteTemplate { get; protected internal set; }

        [CustomizableByCompany]
        [Display(Name = "Hide Charge Type In User Note", Description = "Controls if the charge type is sent to driver as part of the user noteB.")]
        public bool HideChargeTypeInUserNote { get; protected internal set; }

        [Display(Name = "IBS OrderPriority", Description = "IBS OrderPriority")]
        public bool OrderPriority { get; protected internal set; }

        [Display(Name = "IBS ExcludedProviderId", Description = "IBS ExcludedProviderId")]
        public string ExcludedProviderId { get; protected internal set; }

        [Display(Name = "IBS ExcludedVehicleTypeId", Description = "IBS ExcludedVehicleTypeId")]
        public string ExcludedVehicleTypeId { get; protected internal set; }

        [Display(Name = "IBS ZoneByCompanyEnabled", Description = "IBS ZoneByCompanyEnabled")]
        public bool ZoneByCompanyEnabled { get; protected internal set; }

        [Display(Name = "IBS ValidateDestinationZone", Description = "IBS ValidateDestinationZone")]
        public bool ValidateDestinationZone { get; protected internal set; }

        [Display(Name = "IBS ValidatePickupZone", Description = "IBS ValidatePickupZone")]
        public bool ValidatePickupZone { get; protected internal set; }

        [Display(Name = "IBS DestinationZoneToExclude", Description = "IBS DestinationZoneToExclude")]
        public string DestinationZoneToExclude { get; protected internal set; }

        [Display(Name = "IBS PickupZoneToExclude", Description = "IBS PickupZoneToExclude")]
        public string PickupZoneToExclude { get; protected internal set; }
        
        [RequiredAtStartup]
        [Display(Name = "IBS RestApiUrl", Description = "IBS RestApiUrl")]
        public string RestApiUrl { get; set; }

        [RequiredAtStartup]
        [Display(Name = "IBS RestApiUser", Description = "IBS RestApiUser")]
        public string RestApiUser { get; set; }

        [RequiredAtStartup]
        [Display(Name = "IBS RestApiSecret", Description = "IBS RestApiSecret")]
        public string RestApiSecret { get; set; }        
        
        [RequiredAtStartup]
        [Display(Name = "IBS WebServicesUrl", Description = "IBS WebServicesUrl")]
        public string WebServicesUrl { get; set; }

        [RequiredAtStartup]
        [Display(Name = "IBS WebServicesUserName", Description = "IBS WebServicesUserName")]
        public string WebServicesUserName { get; set; }

        [RequiredAtStartup]
        [Display(Name = "IBS WebServicesPassword", Description = "IBS WebServicesPassword")]
        public string WebServicesPassword { get;  set; }

        [CustomizableByCompany]
        public int? PaymentTypeCardOnFileId { get; set; }

        [CustomizableByCompany]
        public int? PaymentTypePaymentInCarId { get; set; }

        [CustomizableByCompany]
        public int? PaymentTypeChargeAccountId { get; set; }
    }

    public class EmailSettingContainer
    {
        [Display(Name = "No Reply Email", Description = "No Reply Email")]
        public string NoReply { get; protected internal set; }

        [CustomizableByCompany]
        [Display(Name = "CC Email Address", Description = "Email address to put in CC when sending an email to a user (booking confirmation only for now)")]
        public string CC { get; protected internal set; }
    }

    public class ReceiptSettingContainer
    {
        [CustomizableByCompany]
        [Display(Name = "Receipt Note", Description = "Receipt Note")]
        public string Note { get; protected internal set; }
    }

    public class CustomerPortalSettingContainer
    {
        [Display(Name = "Customer Portal Url", Description = "Customer Portal Url")]
        public string Url { get; protected internal set; }

        [Display(Name = "Customer Portal UserName", Description = "Customer UserName")]
        public string UserName { get; protected internal set; }

        [Display(Name = "Customer Portal Password", Description = "Customer Portal Password")]
        public string Password { get; protected internal set; }
    }

    public class NetworkSettingContainer
    {
        [CustomizableByCompany, RequiresTaxiHailPro]
        public double PrimaryOrderTimeout { get; protected internal set; }

        [CustomizableByCompany, RequiresTaxiHailPro]
        public double SecondaryOrderTimeout { get; protected internal set; }

        [CustomizableByCompany, RequiresTaxiHailPro, SendToClient]
        [Display(Name = "Hide Market Change Warning", Description = "Hide the warning that is displayed when entering a new market")]
        public bool HideMarketChangeWarning { get; protected internal set; }

        [CustomizableByCompany, RequiresTaxiHailPro, SendToClient]
        [Display(Name = "Auto Confirm Fleet Change", Description = "Automatically change company if timeout occurs when trying to assing a taxi")]
        public bool AutoConfirmFleetChange { get; protected internal set; }

        [SendToClient]
        [Display(Name = "Enable Network", Description = "Is TaxiHailNetwork Enabled")]
        public bool Enabled { get;  set; }
    }

    public class HoneyBadgerSettingContainer
    {
        [Display(Name = "Honey Badger Service Url", Description = "Honey Badger Service Url. N.B.: Market request parameter is added automatically by the middleware, no need to add it here.")]
        public string ServiceUrl { get; protected internal set; }

        [Display(Name = "Available Vehicles Market", Description = "Market used to find vehicles when Available Vehicles Mode is set to 'HoneyBadger'")]
        public string AvailableVehiclesMarket { get; protected internal set; }

        [Display(Name = "Available Vehicles Fleet ID", Description = "Fleet ID used to find vehicles when Available Vehicles Mode is set to 'HoneyBadger'")]
        public int? AvailableVehiclesFleetId { get; protected internal set; }
    }

    public class CmtGeoSettingContainer
    {
        [Display(Name = "Cmt Geo service Url", Description = "Cmt geo Service Url. N.B.: Market request parameter is added automatically by the middleware, no need to add it here.")]
        public string ServiceUrl { get; protected internal set; }

        [Display(Name = "Cmt Geo service API key", Description = "The API key for geo services")]
        public string AppKey { get; protected internal set; }

        [Display(Name = "Cmt Geo Available Vehicles Market", Description = "Market used to find vehicles when Available Vehicles Mode is set to 'Geo'")]
        public string AvailableVehiclesMarket { get; protected internal set; }

        [Display(Name = "Cmt Geo Available Vehicles Fleet ID", Description = "Fleet ID used to find vehicles when Available Vehicles Mode is set to 'Geo'")]
        public int? AvailableVehiclesFleetId { get; protected internal set; }
    }

    public class CmtGdsSettingContainer
    {
        [Display(Name = "GDS Service Url", Description = "GDS Service Url. N.B.: Just the base URL most likely just server and port, should not end in /")]
        public string ServiceUrl { get; protected internal set; }
    }
}
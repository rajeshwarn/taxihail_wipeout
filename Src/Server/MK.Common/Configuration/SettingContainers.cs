using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using apcurium.MK.Common.Entity;
using MK.Common.Configuration;

namespace apcurium.MK.Common.Configuration
{
    public class TaxiHailSettingContainer
    {
        [SendToClient]
        [Display(Name = "Application Name", Description = "Application name as displayed in message")]
        public string ApplicationName { get; protected internal set; }

        public string ApplicationKey { get; protected internal set; }

        public string AccentColor { get; protected internal set; }

        public string EmailFontColor { get; protected internal set; }

        public string SiteName { get; protected internal set; }
    }

    public class OrderStatusSettingContainer
    {
        [SendToClient]
        [Display(Name = "Client Polling Interval", Description = "Status refresh interval")]
        public int ClientPollingInterval { get; protected internal set; }

        public bool DemoMode { get; protected internal set; }

        public int ServerPollingInterval { get; protected internal set; }
    }

    public class GCMSettingContainer
    {
        [SendToClient(ExcludeFromWebApp = true)]
        [Display(Name = "SenderId", Description = "Google Push Notification API Id")]
        public string SenderId { get; protected internal set; }

        public string APIKey { get; protected internal set; }

        public string PackageName { get; protected internal set; }
    }

    public class DirectionSettingContainer
    {
        [SendToClient(ExcludeFromWebApp = true)]
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
        [SendToClient(ExcludeFromWebApp = true)]
        [Display(Name = "Default Radius", Description = "Default radius for places search")]
        public int DefaultRadius { get; protected internal set; }
    }

    public class MapSettingContainer
    {
        [SendToClient(ExcludeFromWebApp = true)]
        [Display(Name = "Place Types", Description = "Give a list of Google Maps places types to filter search")]
        public string PlacesApiKey { get; protected internal set; }
    }

    public class GeoLocSettingContainer
    {
        [SendToClient]
        [Display(Name = "Default Latitude", Description = "Default latitude to display the map before geoloc is done")]
        public double DefaultLatitude { get; protected internal set; }

        [SendToClient]
        [Display(Name = "Default Longitude", Description = "Default longitude to display the map before geoloc is done")]
        public double DefaultLongitude { get; protected internal set; }

        [SendToClient]
        [Display(Name = "Place Types", Description = "Give a list of Google Maps places types to filter search")]
        public string PlacesTypes { get; protected internal set; }

        [SendToClient]
        [Display(Name = "Search Filter", Description = "Filter for geolocation search")]
        public string SearchFilter { get; protected internal set; }
    }

    public class AvailableVehiclesSettingContainer
    {
        [SendToClient]
        public bool Enabled { get; protected internal set; }

        [SendToClient]
        public int Count { get; protected internal set; }

        [SendToClient]
        public int Radius { get; protected internal set; }
    }

    public class AdminSettingContainer
    {
        [Display(Name = "Company Settings", Description = "List of settings that can be modified by the taxi company")]
        public string CompanySettings { get; protected internal set; }
    }

    public class SmtpSettingContainer
    {
        public SmtpSettingContainer()
        {
            Credentials = new CredentialsContainer();
        }

        public string Host { get; protected internal set; }
        public int Port { get; protected internal set; }
        public bool UseDefaultCredentials { get; protected internal set; }
        public bool EnableSsl { get; protected internal set; }
        public SmtpDeliveryMethod DeliveryMethod { get; protected internal set; }
        public CredentialsContainer Credentials { get; protected internal set; }
    }

    public class CredentialsContainer
    {
        public string Username { get; protected internal set; }
        public string Password { get; protected internal set; }
    }

    public class APNSSettingContainer
    {
        public string CertificatePassword { get; protected internal set; }
        public string DevelopmentCertificatePath { get; protected internal set; }
        public string ProductionCertificatePath { get; protected internal set; }
    }

    public class DefaultBookingSettingsSettingContainer
    {
        public int? ChargeTypeId { get; protected internal set; }
        public int NbPassenger { get; protected internal set; }
        public int? ProviderId { get; protected internal set; }
        public int? VehicleTypeId { get; protected internal set; }
    }

    public class StoreSettingContainer
    {
        public string AppleLink { get; protected internal set; }
        public string PlayLink { get; protected internal set; }
    }

    public class IBSSettingContainer
    {
        public bool AutoDispatch { get; protected internal set; }

        public string DefaultAccountPassword { get; protected internal set; }

        public long TimeDifference { get; protected internal set; }

        public bool FakeOrderStatusUpdate { get; protected internal set; }

        public string NoteTemplate { get; protected internal set; }

        public bool OrderPriority { get; protected internal set; }

        public string ExcludedProviderId { get; protected internal set; }

        public string ExcludedVehicleTypeId { get; protected internal set; }

        public bool ZoneByCompanyEnabled { get; protected internal set; }

        public bool ValidateDestinationZone { get; protected internal set; }

        public bool ValidatePickupZone { get; protected internal set; }

        public string DestinationZoneToExclude { get; protected internal set; }

        public string PickupZoneToExclude { get; protected internal set; }

        public string WebServicesPassword { get; protected internal set; }

        public string WebServicesUrl { get; protected internal set; }

        public string WebServicesUserName { get; protected internal set; }
    }

    public class EmailSettingContainer
    {
        public string NoReply { get; protected internal set; }
    }

    public class ReceiptSettingContainer
    {
        public string Note { get; protected internal set; }
    }
}
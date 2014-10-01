using System.ComponentModel.DataAnnotations;

namespace MK.Common.Configuration
{
    public class ServerTaxiHailSetting : TaxiHailSetting
    {
        public ServerTaxiHailSetting()
        {
            Admin = new AdminSettingContainer
            {
                CompanySettings = "Client.ShowEstimateWarning,Client.DestinationIsRequired,IBS.TimeDifference,IBS.PickupZoneToExclude,IBS.DestinationZoneToExclude,IBS.ValidateDestinationZone,IBS.ValidatePickupZone,Client.HideCallDispatchButton,Client.ShowAssignedVehicleNumberOnPin,Client.ZoomOnNearbyVehicles,Client.ZoomOnNearbyVehiclesCount,Client.ZoomOnNearbyVehiclesRadius,DefaultBookingSettings.ChargeTypeId,DefaultBookingSettings.NbPassenger,DefaultBookingSettings.ProviderId,DefaultBookingSettings.VehicleTypeId,Receipt.Note,Client.HideReportProblem,OrderStatus.ServerPollingInterval,IBS.NoteTemplate,AccountActivationDisabled,AvailableVehicles.Enabled,AvailableVehicles.Radius,AvailableVehicles.Count,Store.AppleLink,Store.PlayLink"
            };

            Smtp = new SmtpSettingContainer
            {
                Host = "smtpcorp.com"
            };
        }

        public AdminSettingContainer Admin { get; protected set; }
        public SmtpSettingContainer Smtp { get; protected set; }
        public GCMSettingContainer GCM { get; protected set; }











        public class AdminSettingContainer
        {
            [Display(Name = "Company Settings", Description = "List of settings that can be modified by the taxi company")]
            public string CompanySettings { get; protected internal set; }
        }

        public class SmtpSettingContainer
        {
            public string Host { get; protected internal set; }
        }

        public partial class GCMSettingContainer
        {
            public string APIKey { get; protected set; }
            public string PackageName { get; protected set; }
        }
    }
}
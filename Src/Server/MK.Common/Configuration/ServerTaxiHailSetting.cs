using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;

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
                Credentials = new SmtpSettingContainer.CredentialsContainer
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
        }

        public AdminSettingContainer Admin { get; protected set; }
        public SmtpSettingContainer Smtp { get; protected set; }
        public APNSSettingContainer APNS { get; protected set; }
        public DefaultBookingSettingsSettingContainer DefaultBookingSettings { get; protected set; }
        public StoreSettingContainer Store { get; protected set; }
        public IBSSettingContainer IBS { get; protected set; }
        public EmailSettingContainer Email { get; protected set; }
        public ReceiptSettingContainer Receipt { get; protected set; }
    }
}
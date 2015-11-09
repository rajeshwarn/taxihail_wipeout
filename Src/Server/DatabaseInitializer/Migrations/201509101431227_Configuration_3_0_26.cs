using System.Data.SqlClient;
using DatabaseInitializer;

namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_3_0_26 : DbMigration
    {
        public override void Up()
        {
            //dirty but there's no easy way to pass argument to a migration
            if (Program.IsUpdate)
            {
                return;
            }
                CreateTable(
                    "Config.NotificationSettings",
                    c => new
                    {
                        Id = c.Guid(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        BookingConfirmationEmail = c.Boolean(),
                        ReceiptEmail = c.Boolean(),
                        PromotionUnlockedEmail = c.Boolean(),
                        DriverAssignedPush = c.Boolean(),
                        ConfirmPairingPush = c.Boolean(),
                        NearbyTaxiPush = c.Boolean(),
                        UnpairingReminderPush = c.Boolean(),
                        VehicleAtPickupPush = c.Boolean(),
                        PaymentConfirmationPush = c.Boolean(),
                        PromotionUnlockedPush = c.Boolean()
                    })
                    .PrimaryKey(t => t.Id);

                CreateTable(
                    "Config.PaymentSettings",
                    c => new
                    {
                        Id = c.Guid(nullable: false),
                        CompanyKey = c.String(),
                        BraintreeServerSettings_IsSandbox = c.Boolean(nullable: false),
                        BraintreeServerSettings_MerchantId = c.String(),
                        BraintreeServerSettings_PublicKey = c.String(),
                        BraintreeServerSettings_PrivateKey = c.String(),
                        PayPalServerSettings_SandboxCredentials_Secret = c.String(),
                        PayPalServerSettings_SandboxCredentials_MerchantId = c.String(),
                        PayPalServerSettings_Credentials_Secret = c.String(),
                        PayPalServerSettings_Credentials_MerchantId = c.String(),
                        PayPalServerSettings_LandingPageType = c.Int(nullable: false),
                        NoShowFee = c.Decimal(precision: 18, scale: 2),
                        IsPreAuthEnabled = c.Boolean(nullable: false),
                        PreAuthAmount = c.Decimal(precision: 18, scale: 2),
                        IsUnpairingDisabled = c.Boolean(nullable: false),
                        UnpairingTimeOut = c.Int(nullable: false),
                        IsPrepaidEnabled = c.Boolean(nullable: false),
                        AlwaysDisplayCoFOption = c.Boolean(nullable: false),
                        PaymentMode = c.Int(nullable: false),
                        IsPayInTaxiEnabled = c.Boolean(nullable: false),
                        IsOutOfAppPaymentDisabled = c.Boolean(nullable: false),
                        IsChargeAccountPaymentEnabled = c.Boolean(nullable: false),
                        AutomaticPaymentPairing = c.Boolean(nullable: false),
                        AskForCVVAtBooking = c.Boolean(nullable: false),
                        CmtPaymentSettings_IsManualRidelinqCheckInEnabled = c.Boolean(nullable: false),
                        CmtPaymentSettings_IsSandbox = c.Boolean(nullable: false),
                        CmtPaymentSettings_BaseUrl = c.String(),
                        CmtPaymentSettings_SandboxBaseUrl = c.String(),
                        CmtPaymentSettings_MobileBaseUrl = c.String(),
                        CmtPaymentSettings_SandboxMobileBaseUrl = c.String(),
                        CmtPaymentSettings_ConsumerSecretKey = c.String(),
                        CmtPaymentSettings_ConsumerKey = c.String(),
                        CmtPaymentSettings_FleetToken = c.String(),
                        CmtPaymentSettings_CurrencyCode = c.String(),
                        CmtPaymentSettings_Market = c.String(),
                        CmtPaymentSettings_SubmitAsFleetAuthorization = c.Boolean(nullable: false),
                        CmtPaymentSettings_MerchantToken = c.String(),
                        BraintreeClientSettings_ClientKey = c.String(),
                        MonerisPaymentSettings_IsSandbox = c.Boolean(nullable: false),
                        MonerisPaymentSettings_BaseHost = c.String(),
                        MonerisPaymentSettings_SandboxHost = c.String(),
                        MonerisPaymentSettings_StoreId = c.String(),
                        MonerisPaymentSettings_ApiToken = c.String(),
                        PayPalClientSettings_IsEnabled = c.Boolean(nullable: false),
                        PayPalClientSettings_IsSandbox = c.Boolean(nullable: false),
                        PayPalClientSettings_SandboxCredentials_ClientId = c.String(),
                        PayPalClientSettings_Credentials_ClientId = c.String(),
                    })
                    .PrimaryKey(t => t.Id);

                CreateTable(
                    "Config.UserTaxiHailNetworkSettings",
                    c => new
                    {
                        Id = c.Guid(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                        SerializedDisabledFleets = c.String(),
                    })
                    .PrimaryKey(t => t.Id);

                CreateTable(
                    "Config.AppSettings",
                    c => new
                    {
                        Key = c.String(nullable: false, maxLength: 128),
                        Value = c.String(),
                    })
                    .PrimaryKey(t => t.Key);
            
        }
        
        public override void Down()
        {
            
        }
    }
}

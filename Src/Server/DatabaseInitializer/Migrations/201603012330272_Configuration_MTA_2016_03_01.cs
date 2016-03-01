namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MTA_2016_03_01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.NotificationSettings", "NoShowPush", c => c.Boolean());
            AddColumn("Config.PaymentSettings", "BraintreeServerSettings_MerchantAccountId", c => c.String());
            AddColumn("Config.PaymentSettings", "IsPaymentOutOfAppDisabled", c => c.Int(nullable: false));
            AddColumn("Config.PaymentSettings", "DisableAMEX", c => c.Boolean(nullable: false));
            AddColumn("Config.PaymentSettings", "CreditCardIsMandatory", c => c.Boolean(nullable: false));
            AddColumn("Config.PaymentSettings", "CmtPaymentSettings_PairingMethod", c => c.Int(nullable: false));
            AddColumn("Config.PaymentSettings", "BraintreeClientSettings_EnablePayPal", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "BraintreeClientSettings_EnablePayPal");
            DropColumn("Config.PaymentSettings", "CmtPaymentSettings_PairingMethod");
            DropColumn("Config.PaymentSettings", "CreditCardIsMandatory");
            DropColumn("Config.PaymentSettings", "DisableAMEX");
            DropColumn("Config.PaymentSettings", "IsPaymentOutOfAppDisabled");
            DropColumn("Config.PaymentSettings", "BraintreeServerSettings_MerchantAccountId");
            DropColumn("Config.NotificationSettings", "NoShowPush");
        }
    }
}

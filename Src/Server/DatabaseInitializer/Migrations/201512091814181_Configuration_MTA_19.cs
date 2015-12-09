namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MTA_19 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "CmtPaymentSettings_ConsumerSecretKeyLuxury", c => c.String());
            AddColumn("Config.PaymentSettings", "CmtPaymentSettings_ConsumerKeyLuxury", c => c.String());
            AddColumn("Config.PaymentSettings", "CmtPaymentSettings_FleetTokenLuxury", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "CmtPaymentSettings_FleetTokenLuxury");
            DropColumn("Config.PaymentSettings", "CmtPaymentSettings_ConsumerKeyLuxury");
            DropColumn("Config.PaymentSettings", "CmtPaymentSettings_ConsumerSecretKeyLuxury");
        }
    }
}

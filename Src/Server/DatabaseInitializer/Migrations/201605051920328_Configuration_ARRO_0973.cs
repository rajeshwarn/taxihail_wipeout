namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_ARRO_0973 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "CmtPaymentSettings_TokenizeValidateFrequencyThresholdInHours", c => c.Int(nullable: false, defaultValue: 6));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "CmtPaymentSettings_TokenizeValidateFrequencyThresholdInHours");
        }
    }
}

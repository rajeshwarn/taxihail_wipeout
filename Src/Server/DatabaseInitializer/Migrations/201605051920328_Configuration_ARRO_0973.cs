namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_ARRO_0973 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "CmtPaymentSettings_TokenizeValidateFrequencyThresholdInHours", c => c.Int(nullable: true, defaultValue: 6));

            Sql("UPDATE Config.PaymentSettings SET CmtPaymentSettings_TokenizeValidateFrequencyThresholdInHours = '6' WHERE CmtPaymentSettings_TokenizeValidateFrequencyThresholdInHours IS NULL");
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "CmtPaymentSettings_TokenizeValidateFrequencyThresholdInHours");
        }
    }
}

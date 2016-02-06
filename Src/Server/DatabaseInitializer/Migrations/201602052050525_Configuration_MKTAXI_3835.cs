namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MKTAXI_3835 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "CmtPaymentSettings_PairingMethod", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "CmtPaymentSettings_PairingMethod");
        }
    }
}

namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MKTAXI_3005 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "BraintreeClientSettings_EnablePayPal", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "BraintreeClientSettings_EnablePayPal");
        }
    }
}

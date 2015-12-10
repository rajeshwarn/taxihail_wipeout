namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MKTAXI_3640 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "IsPaymentOutOfAppDisabled", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "IsPaymentOutOfAppDisabled");
        }
    }
}

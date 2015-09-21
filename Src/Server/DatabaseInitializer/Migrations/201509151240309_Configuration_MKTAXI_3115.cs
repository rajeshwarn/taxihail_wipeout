namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MKTAXI_3115 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "CancelOrderOnUnpair", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "CancelOrderOnUnpair");
        }
    }
}

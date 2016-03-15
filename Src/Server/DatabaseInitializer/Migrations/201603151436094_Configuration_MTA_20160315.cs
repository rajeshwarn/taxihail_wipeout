namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MTA_20160315 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "DisableVisaMastercard", c => c.Boolean(nullable: false));
            AddColumn("Config.PaymentSettings", "DisableDiscover", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "DisableDiscover");
            DropColumn("Config.PaymentSettings", "DisableVisaMastercard");
        }
    }
}

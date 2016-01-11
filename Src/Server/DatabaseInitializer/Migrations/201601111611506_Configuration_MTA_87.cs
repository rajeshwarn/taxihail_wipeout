namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MTA_87 : DbMigration
    {
        public override void Up()
        {
            DropColumn("Config.ServiceTypeSettings", "WaitTimeRatePerMinute");
        }
        
        public override void Down()
        {
            AddColumn("Config.ServiceTypeSettings", "WaitTimeRatePerMinute", c => c.Double(nullable: false));
        }
    }
}

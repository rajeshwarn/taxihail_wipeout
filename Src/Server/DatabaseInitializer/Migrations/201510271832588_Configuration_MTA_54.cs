namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MTA_54 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.ServiceTypeSettings", "WaitTimeRatePerMinute", c => c.Double(nullable: false));
            AddColumn("Config.ServiceTypeSettings", "AirportMeetAndGreetRate", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Config.ServiceTypeSettings", "AirportMeetAndGreetRate");
            DropColumn("Config.ServiceTypeSettings", "WaitTimeRatePerMinute");
        }
    }
}

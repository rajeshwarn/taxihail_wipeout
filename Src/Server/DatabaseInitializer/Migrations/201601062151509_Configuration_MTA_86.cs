namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MTA_86 : DbMigration
    {
        public override void Up()
        {
            DropColumn("Config.ServiceTypeSettings", "AirportMeetAndGreetRate");
        }
        
        public override void Down()
        {
            AddColumn("Config.ServiceTypeSettings", "AirportMeetAndGreetRate", c => c.Double(nullable: false));
        }
    }
}

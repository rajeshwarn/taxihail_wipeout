namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_Revert_MTA_5 : DbMigration
    {
        public override void Up()
        {
            DropColumn("Config.ServiceTypeSettings", "FutureBookingThresholdInMinutes");
        }
        
        public override void Down()
        {
            AddColumn("Config.ServiceTypeSettings", "FutureBookingThresholdInMinutes", c => c.Int(nullable: false));
        }
    }
}

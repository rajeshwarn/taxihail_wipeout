namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MKTAXI3415 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.NotificationSettings", "DriverBailedPush", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("Config.NotificationSettings", "DriverBailedPush");
        }
    }
}

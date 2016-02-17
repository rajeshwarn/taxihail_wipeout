namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MKTAXI_3873 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.NotificationSettings", "NoShowPush", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("Config.NotificationSettings", "NoShowPush");
        }
    }
}

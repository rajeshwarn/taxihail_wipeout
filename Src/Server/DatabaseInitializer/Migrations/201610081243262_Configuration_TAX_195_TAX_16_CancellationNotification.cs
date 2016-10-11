namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_TAX_195_TAX_16_CancellationNotification : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.NotificationSettings", "OrderCancellationConfirmationPush", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("Config.NotificationSettings", "OrderCancellationConfirmationPush");
        }
    }
}

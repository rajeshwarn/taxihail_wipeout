namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MTA_73 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.ServiceTypeSettings", "ProviderId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Config.ServiceTypeSettings", "ProviderId");
        }
    }
}

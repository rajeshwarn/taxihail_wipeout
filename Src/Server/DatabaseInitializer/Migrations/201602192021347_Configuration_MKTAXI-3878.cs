namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MKTAXI3878 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "DisableAMEX", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "DisableAMEX");
        }
    }
}

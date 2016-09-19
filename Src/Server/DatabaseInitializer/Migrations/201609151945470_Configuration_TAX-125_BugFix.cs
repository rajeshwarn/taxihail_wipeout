namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_TAX125_BugFix : DbMigration
    {
        public override void Up()
        {
            DropColumn("Config.PaymentSettings", "EnableContactVerification");
        }
        
        public override void Down()
        {
            AddColumn("Config.PaymentSettings", "EnableContactVerification", c => c.Boolean(nullable: false));
        }
    }
}

namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_TAX_125 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "EnableAddressVerification", c => c.Boolean(nullable: false));
            AddColumn("Config.PaymentSettings", "EnableContactVerification", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "EnableContactVerification");
            DropColumn("Config.PaymentSettings", "EnableAddressVerification");
        }
    }
}

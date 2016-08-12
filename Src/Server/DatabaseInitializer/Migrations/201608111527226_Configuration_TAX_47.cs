namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_TAX_47 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "MonerisPaymentSettings_UseCarIdInTransaction", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "MonerisPaymentSettings_UseCarIdInTransaction");
        }
    }
}

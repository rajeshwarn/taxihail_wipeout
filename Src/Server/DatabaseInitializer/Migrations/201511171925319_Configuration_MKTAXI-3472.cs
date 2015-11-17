namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MKTAXI3472 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "CreditCardIsMandatory", c => c.Boolean(nullable: false, defaultValue:false));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "CreditCardIsMandatory");
        }
    }
}

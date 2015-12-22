using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MKTAXI_3640 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Config.PaymentSettings", "IsPaymentOutOfAppDisabled", c => c.Int(nullable: false, defaultValue: (int?)OutOfAppPaymentDisabled.NotSet));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "IsPaymentOutOfAppDisabled");
        }
    }
}

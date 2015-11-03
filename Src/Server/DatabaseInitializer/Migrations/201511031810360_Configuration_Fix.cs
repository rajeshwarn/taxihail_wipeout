namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_Fix : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Config.ServiceTypeSettings",
                c => new
                    {
                        ServiceType = c.Int(nullable: false),
                        IBSWebServicesUrl = c.String(),
                        FutureBookingThresholdInMinutes = c.Int(nullable: false),
                        WaitTimeRatePerMinute = c.Double(nullable: false),
                        AirportMeetAndGreetRate = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.ServiceType);
            
            AddColumn("Config.PaymentSettings", "CmtPaymentSettings_UsePairingCode", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Config.PaymentSettings", "CmtPaymentSettings_UsePairingCode");
            DropTable("Config.ServiceTypeSettings");
        }
    }
}

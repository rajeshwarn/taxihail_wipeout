namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_MTA_25 : DbMigration
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
                    })
                .PrimaryKey(t => t.ServiceType);
            
        }
        
        public override void Down()
        {
            DropTable("Config.ServiceTypeSettings");
        }
    }
}

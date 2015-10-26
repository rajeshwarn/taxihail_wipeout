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
                        Id = c.Guid(nullable: false),
                        ServiceType = c.Int(nullable: false),
                        IBSWebServicesUrl = c.String(),
                        FutureBookingThresholdInMinutes = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("Config.ServiceTypeSettings");
        }
    }
}

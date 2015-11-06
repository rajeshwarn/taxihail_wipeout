namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3456 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Booking.DispatcherSettingsDetail",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Market = c.String(),
                        NumberOfOffersPerCycle = c.Int(nullable: false),
                        NumberOfCycles = c.Int(nullable: false),
                        DurationOfOfferInSeconds = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("Booking.DispatcherSettingsDetail");
        }
    }
}

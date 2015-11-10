namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3466 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Booking.VehicleIdMappingDetail",
                c => new
                    {
                        OrderId = c.Guid(nullable: false),
                        LegacyDispatchId = c.String(),
                        DeviceName = c.String(),
                        CreationDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OrderId);
            
        }
        
        public override void Down()
        {
            DropTable("Booking.VehicleIdMappingDetail");
        }
    }
}

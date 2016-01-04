namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3456 : DbMigration
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
            
            AddColumn("Booking.OrderDetail", "CompanyFleetId", c => c.Int());
            AddColumn("Booking.OrderReportDetail", "Order_CompanyFleetId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderReportDetail", "Order_CompanyFleetId");
            DropColumn("Booking.OrderDetail", "CompanyFleetId");
            DropTable("Booking.VehicleIdMappingDetail");
        }
    }
}

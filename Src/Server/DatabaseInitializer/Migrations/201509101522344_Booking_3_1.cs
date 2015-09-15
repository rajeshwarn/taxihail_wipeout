namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_3_1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Booking.Airline",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "Booking.PickupPoint",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        AdditionalFee = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("Booking.OrderReportDetail", "Order_Error", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderReportDetail", "Order_Error");
            DropTable("Booking.PickupPoint");
            DropTable("Booking.Airline");
        }
    }
}

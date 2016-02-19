namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3873 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderReportDetail", "OrderStatus_OrderIsNoShow", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderReportDetail", "OrderStatus_OrderIsNoShow");
        }
    }
}

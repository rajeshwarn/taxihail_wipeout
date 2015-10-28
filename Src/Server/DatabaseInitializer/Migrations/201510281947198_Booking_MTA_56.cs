namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MTA_56 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderReportDetail", "Order_ServiceType", c => c.Int(nullable: false));
            DropColumn("Booking.OrderStatusDetail", "ServiceType");
        }
        
        public override void Down()
        {
            AddColumn("Booking.OrderStatusDetail", "ServiceType", c => c.Int(nullable: false));
            DropColumn("Booking.OrderReportDetail", "Order_ServiceType");
        }
    }
}

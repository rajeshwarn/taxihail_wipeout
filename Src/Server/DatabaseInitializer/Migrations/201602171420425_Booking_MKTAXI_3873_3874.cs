namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3873_3874 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderStatusDetail", "LastTripPollingDateInUtc", c => c.DateTime());
            AddColumn("Booking.OrderManualRideLinqDetail", "IsWaitingForPayment", c => c.Boolean(nullable: false));
            AddColumn("Booking.OrderReportDetail", "OrderStatus_OrderIsNoShow", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderReportDetail", "OrderStatus_OrderIsNoShow");
            DropColumn("Booking.OrderManualRideLinqDetail", "IsWaitingForPayment");
            DropColumn("Booking.OrderStatusDetail", "LastTripPollingDateInUtc");
        }
    }
}

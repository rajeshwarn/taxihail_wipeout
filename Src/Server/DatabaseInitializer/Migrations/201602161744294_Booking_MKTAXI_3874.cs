namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3874 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderStatusDetail", "LastTripPollingDateInUtc", c => c.DateTime());
            AddColumn("Booking.OrderManualRideLinqDetail", "IsWaitingForPayment", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderManualRideLinqDetail", "IsWaitingForPayment");
            DropColumn("Booking.OrderStatusDetail", "LastTripPollingDateInUtc");
        }
    }
}

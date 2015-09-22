namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3151 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderManualRideLinqDetail", "PairingError", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderManualRideLinqDetail", "PairingError");
        }
    }
}

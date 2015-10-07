namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_2905 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderManualRideLinqDetail", "DeviceName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderManualRideLinqDetail", "DeviceName");
        }
    }
}

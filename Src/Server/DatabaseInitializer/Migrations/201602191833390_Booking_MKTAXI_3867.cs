namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3867 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderManualRideLinqDetail", "LastLatitudeOfVehicle", c => c.Double());
            AddColumn("Booking.OrderManualRideLinqDetail", "LastLongitudeOfVehicle", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderManualRideLinqDetail", "LastLongitudeOfVehicle");
            DropColumn("Booking.OrderManualRideLinqDetail", "LastLatitudeOfVehicle");
        }
    }
}

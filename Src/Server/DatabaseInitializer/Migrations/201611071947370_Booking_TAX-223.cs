namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_TAX223 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.AccountDetail", "Settings_LuxuryVehicleTypeId", c => c.Int());
            AddColumn("Booking.AccountDetail", "Settings_LuxuryVehicleType", c => c.String());
            AddColumn("Booking.OrderDetail", "Settings_LuxuryVehicleTypeId", c => c.Int());
            AddColumn("Booking.OrderDetail", "Settings_LuxuryVehicleType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderDetail", "Settings_LuxuryVehicleType");
            DropColumn("Booking.OrderDetail", "Settings_LuxuryVehicleTypeId");
            DropColumn("Booking.AccountDetail", "Settings_LuxuryVehicleType");
            DropColumn("Booking.AccountDetail", "Settings_LuxuryVehicleTypeId");
        }
    }
}

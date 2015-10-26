namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MTA_50 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.VehicleTypeDetail", "ServiceType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.VehicleTypeDetail", "ServiceType");
        }
    }
}

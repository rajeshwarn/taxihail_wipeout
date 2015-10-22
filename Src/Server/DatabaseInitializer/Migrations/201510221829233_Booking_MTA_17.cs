namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MTA_17 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.AccountDetail", "Settings_ServiceType", c => c.Int(nullable: false));
            AddColumn("Booking.OrderDetail", "Settings_ServiceType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderDetail", "Settings_ServiceType");
            DropColumn("Booking.AccountDetail", "Settings_ServiceType");
        }
    }
}

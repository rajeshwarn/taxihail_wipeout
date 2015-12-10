namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MTA_79 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.TariffDetail", "ServiceType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.TariffDetail", "ServiceType");
        }
    }
}

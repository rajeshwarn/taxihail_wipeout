namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI3654 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderDetail", "IsRefunded", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderDetail", "IsRefunded");
        }
    }
}

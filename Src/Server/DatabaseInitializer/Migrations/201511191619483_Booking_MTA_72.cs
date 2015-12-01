namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MTA_72 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderDetail", "Gratuity", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderDetail", "Gratuity");
        }
    }
}

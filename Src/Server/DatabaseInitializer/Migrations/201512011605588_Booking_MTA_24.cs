namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MTA_24 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderNotificationDetail", "InfoAboutGratuitySent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderNotificationDetail", "InfoAboutGratuitySent");
        }
    }
}

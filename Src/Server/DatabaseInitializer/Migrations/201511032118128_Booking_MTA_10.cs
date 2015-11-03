namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MTA_10 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderStatusDetail", "NoShowStartTime", c => c.DateTime());
            AddColumn("Booking.OrderNotificationDetail", "NoShowWarningSent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderNotificationDetail", "NoShowWarningSent");
            DropColumn("Booking.OrderStatusDetail", "NoShowStartTime");
        }
    }
}

namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_Mears_Sept8ExtraGratuity : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderStatusDetail", "WaitingForExtraGratuityStartDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderStatusDetail", "WaitingForExtraGratuityStartDate");
        }
    }
}

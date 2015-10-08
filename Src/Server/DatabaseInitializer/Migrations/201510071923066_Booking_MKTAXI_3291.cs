namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3291 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderStatusDetail", "TipIncentive", c => c.Double());
            AddColumn("Booking.OrderDetail", "TipIncentive", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderDetail", "TipIncentive");
            DropColumn("Booking.OrderStatusDetail", "TipIncentive");
        }
    }
}

namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3815 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderStatusUpdateDetail", "CycleStartDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderStatusUpdateDetail", "CycleStartDate");
        }
    }
}

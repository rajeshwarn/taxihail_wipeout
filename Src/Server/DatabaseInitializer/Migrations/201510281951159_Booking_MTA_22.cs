namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MTA_22 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderReportDetail", "Order_ServiceType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderReportDetail", "Order_ServiceType");
        }
    }
}

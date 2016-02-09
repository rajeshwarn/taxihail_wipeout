namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3711 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderDetail", "OriginatingIpAddress", c => c.String());
            AddColumn("Booking.OrderDetail", "KountSessionId", c => c.String());
            AddColumn("Booking.OrderReportDetail", "Order_OriginatingIpAddress", c => c.String());
            AddColumn("Booking.OrderReportDetail", "Order_KountSessionId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderReportDetail", "Order_KountSessionId");
            DropColumn("Booking.OrderReportDetail", "Order_OriginatingIpAddress");
            DropColumn("Booking.OrderDetail", "KountSessionId");
            DropColumn("Booking.OrderDetail", "OriginatingIpAddress");
        }
    }
}

namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3115 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderPairingDetail", "WasUnpaired", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("Booking.OrderReportDetail", "Payment_WasUnpaired", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderReportDetail", "Payment_WasUnpaired");
            DropColumn("Booking.OrderPairingDetail", "WasUnpaired");
        }
    }
}

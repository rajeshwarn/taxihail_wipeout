namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3649 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderReportDetail", "Payment_DriverId", c => c.String());
            AddColumn("Booking.OrderReportDetail", "Payment_Medaillon", c => c.String());
            AddColumn("Booking.OrderReportDetail", "Payment_Last4Digits", c => c.String());
            AddColumn("Booking.OrderReportDetail", "VehicleInfos_DriverId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderReportDetail", "VehicleInfos_DriverId");
            DropColumn("Booking.OrderReportDetail", "Payment_Last4Digits");
            DropColumn("Booking.OrderReportDetail", "Payment_Medaillon");
            DropColumn("Booking.OrderReportDetail", "Payment_DriverId");
        }
    }
}

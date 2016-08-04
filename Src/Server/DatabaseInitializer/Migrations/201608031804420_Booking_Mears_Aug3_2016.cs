namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_Mears_Aug3_2016 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderDetail", "Extra", c => c.Double());
            AddColumn("Booking.OrderPaymentDetail", "Extra", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("Booking.OrderReportDetail", "Payment_MdtExtra", c => c.Double());
            AddColumn("Booking.OrderReportDetail", "Payment_MdtSurcharge", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderReportDetail", "Payment_MdtSurcharge");
            DropColumn("Booking.OrderReportDetail", "Payment_MdtExtra");
            DropColumn("Booking.OrderPaymentDetail", "Extra");
            DropColumn("Booking.OrderDetail", "Extra");
        }
    }
}

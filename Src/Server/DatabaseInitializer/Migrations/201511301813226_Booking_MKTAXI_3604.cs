namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3604 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderStatusDetail", "ChargeAmountsTimeOut", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderStatusDetail", "ChargeAmountsTimeOut");
        }
    }
}

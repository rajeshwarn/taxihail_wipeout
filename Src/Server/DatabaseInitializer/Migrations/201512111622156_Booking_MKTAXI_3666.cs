namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3666 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.AccountDetail", "BraintreeAccountId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Booking.AccountDetail", "BraintreeAccountId");
        }
    }
}

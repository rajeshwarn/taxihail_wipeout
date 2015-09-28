namespace apcurium.MK.Booking.Migrations
{
    using Common;
    using System;
    using System.Data.Entity.Migrations;

    public partial class Booking_MKTAXI_3199 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.CreditCardDetails", "Label", c => c.String(false, null, null, null, CreditCardConstants.Personal));
        }
        
        public override void Down()
        {
            DropColumn("Booking.CreditCardDetails", "Label");
        }
    }
}

namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using apcurium.MK.Common;

    public partial class Booking_MKTAXI2773 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.CreditCardDetails", "Label", c => c.String(false, null, null, null, CreditCardLabelConstants.Personal.ToString()));
            AddColumn("Booking.CreditCardDetails", "ZipCode", c => c.String());
        }

        public override void Down()
        {
            DropColumn("Booking.CreditCardDetails", "ZipCode");
            DropColumn("Booking.CreditCardDetails", "Label");
        }
    }
}

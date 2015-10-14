namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI2773 : DbMigration
    {
        public override void Up()
        {
            DropColumn("Booking.CreditCardDetails", "Label");
            DropColumn("Booking.CreditCardDetails", "ZipCode");
        }
        
        public override void Down()
        {
            AddColumn("Booking.CreditCardDetails", "ZipCode", c => c.String());
            AddColumn("Booking.CreditCardDetails", "Label", c => c.String());
        }
    }
}

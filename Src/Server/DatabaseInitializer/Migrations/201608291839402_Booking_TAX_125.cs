namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_TAX_125 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.CreditCardDetails", "StreetNumber", c => c.String());
            AddColumn("Booking.CreditCardDetails", "StreetName", c => c.String());
            AddColumn("Booking.CreditCardDetails", "Email", c => c.String());
            AddColumn("Booking.CreditCardDetails", "Phone", c => c.String());
            AddColumn("Booking.CreditCardDetails", "Country_Code", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Booking.CreditCardDetails", "Country_Code");
            DropColumn("Booking.CreditCardDetails", "Phone");
            DropColumn("Booking.CreditCardDetails", "Email");
            DropColumn("Booking.CreditCardDetails", "StreetName");
            DropColumn("Booking.CreditCardDetails", "StreetNumber");
        }
    }
}

namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_TAX_125 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.CreditCardDetails", "StreetNumber", c => c.String(nullable: true));
            AddColumn("Booking.CreditCardDetails", "StreetName", c => c.String(nullable: true));
            AddColumn("Booking.CreditCardDetails", "Email", c => c.String(nullable: true));
            AddColumn("Booking.CreditCardDetails", "Phone", c => c.String(nullable: true));
            AddColumn("Booking.CreditCardDetails", "Country_Code", c => c.String(nullable: true));
            AlterColumn("Booking.CreditCardDetails", "Label", builder => builder.String(nullable: true));
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

namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI3216 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.CreditCardDetails", "ZipCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Booking.CreditCardDetails", "ZipCode");
        }
    }
}

namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_ARRO_0973 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.CreditCardDetails", "LastTokenValidateDateTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("Booking.CreditCardDetails", "LastTokenValidateDateTime");
        }
    }
}

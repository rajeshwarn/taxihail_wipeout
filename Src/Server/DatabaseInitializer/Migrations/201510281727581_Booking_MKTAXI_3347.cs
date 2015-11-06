namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3347 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.RatingScoreDetails", "AccountId", c => c.Guid(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("Booking.RatingScoreDetails", "AccountId");
        }
    }
}
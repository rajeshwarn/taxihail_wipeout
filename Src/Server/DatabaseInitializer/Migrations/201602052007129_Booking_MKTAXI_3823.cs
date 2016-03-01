namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3823 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderRatingDetails", "AccountId", c => c.Guid());
            DropColumn("Booking.RatingScoreDetails", "AccountId");
        }
        
        public override void Down()
        {
            AddColumn("Booking.RatingScoreDetails", "AccountId", c => c.Guid(nullable: true));
            DropColumn("Booking.OrderRatingDetails", "AccountId");
        }
    }
}

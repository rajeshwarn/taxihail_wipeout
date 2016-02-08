namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3815_3654_3823 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.OrderStatusUpdateDetail", "CycleStartDate", c => c.DateTime());
            AddColumn("Booking.OrderDetail", "IsRefunded", c => c.Boolean(nullable: false));
            AddColumn("Booking.OrderRatingDetails", "AccountId", c => c.Guid());
            DropColumn("Booking.RatingScoreDetails", "AccountId");
        }
        
        public override void Down()
        {
            AddColumn("Booking.RatingScoreDetails", "AccountId", c => c.Guid(nullable: false));
            DropColumn("Booking.OrderRatingDetails", "AccountId");
            DropColumn("Booking.OrderDetail", "IsRefunded");
            DropColumn("Booking.OrderStatusUpdateDetail", "CycleStartDate");
        }
    }
}

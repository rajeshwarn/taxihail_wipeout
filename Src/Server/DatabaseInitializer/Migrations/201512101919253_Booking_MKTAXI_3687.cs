namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3687 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Booking.BlackListEntry",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PhoneNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropTable("Booking.BlackListEntry");
        }
    }
}

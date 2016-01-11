namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3648 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Booking.ConfigurationChangeEntry",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AccountId = c.Guid(nullable: false),
                        AccountEmail = c.String(),
                        Date = c.DateTime(nullable: false),
                        OldValues = c.String(),
                        NewValues = c.String(),
                        Type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("Booking.ConfigurationChangeEntry");
        }
    }
}

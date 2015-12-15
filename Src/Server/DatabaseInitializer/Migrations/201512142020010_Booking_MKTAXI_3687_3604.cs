namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3687_3604 : DbMigration
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
            
            AddColumn("Booking.OrderStatusDetail", "ChargeAmountsTimeOut", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("Booking.OrderStatusDetail", "ChargeAmountsTimeOut");
            DropTable("Booking.BlackListEntry");
        }
    }
}

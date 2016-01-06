namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI3652 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Booking.AccountNoteDetail",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AccountId = c.Guid(nullable: false),
                        Note = c.String(),
                        Type = c.Int(nullable: false),
                        WriterEmail = c.String(),
                        CreationDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("Booking.AccountNoteDetail");
        }
    }
}

namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI3818 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.AccountNoteEntry", "WriterAccountId", c => c.Guid(nullable: false));
            AddColumn("Booking.AccountNoteEntry", "WriterAccountEmail", c => c.String());
            DropColumn("Booking.AccountNoteEntry", "AccountEmail");
        }
        
        public override void Down()
        {
            AddColumn("Booking.AccountNoteEntry", "AccountEmail", c => c.String());
            DropColumn("Booking.AccountNoteEntry", "WriterAccountEmail");
            DropColumn("Booking.AccountNoteEntry", "WriterAccountId");
        }
    }
}

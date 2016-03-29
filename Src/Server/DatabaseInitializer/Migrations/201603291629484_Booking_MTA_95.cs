namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MTA_95 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.RuleDetail", "AppliesToServiceTaxi", c => c.Boolean(nullable: false));
            AddColumn("Booking.RuleDetail", "AppliesToServiceLuxury", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.RuleDetail", "AppliesToServiceLuxury");
            DropColumn("Booking.RuleDetail", "AppliesToServiceTaxi");
        }
    }
}

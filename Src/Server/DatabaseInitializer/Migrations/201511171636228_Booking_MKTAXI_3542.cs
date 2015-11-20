namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MKTAXI_3542 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Booking.RuleDetail", "ExcludeCircularZone", c => c.Boolean(nullable: false));
            AddColumn("Booking.RuleDetail", "ExcludedCircularZoneLatitude", c => c.Double(nullable: false));
            AddColumn("Booking.RuleDetail", "ExcludedCircularZoneLongitude", c => c.Double(nullable: false));
            AddColumn("Booking.RuleDetail", "ExcludedCircularZoneRadius", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Booking.RuleDetail", "ExcludedCircularZoneRadius");
            DropColumn("Booking.RuleDetail", "ExcludedCircularZoneLongitude");
            DropColumn("Booking.RuleDetail", "ExcludedCircularZoneLatitude");
            DropColumn("Booking.RuleDetail", "ExcludeCircularZone");
        }
    }
}

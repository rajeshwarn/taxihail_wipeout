namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class ConfigMigrationBookingContext : DbMigrationsConfiguration<apcurium.MK.Booking.Database.BookingDbContext>
    {
        public ConfigMigrationBookingContext()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(apcurium.MK.Booking.Database.BookingDbContext context)
        {
            
        }
    }
}

using System.Data.Entity.Migrations;

namespace MK.ConfigurationManager
{
    class SimpleDbMigrationsConfiguration  : DbMigrationsConfiguration<ConfigurationManagerDbContext>
    {
        public SimpleDbMigrationsConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            
        }
    }
}
As a new developer, you may start by restoring an existing database. After the restore, use these commands to get your database in synch with the most current migration:

The commands can be entered in the nuget package manager console
Tools -> Nuget Package Manager -> Package Manager Console

**Select the Tools\DatabaseInitializer as the default project

Update-Database -ConfigurationTypeName ConfigMigrationConfigurationContext -StartUpProjectName apcurium.MK.Web -ConnectionStringName MKWeb -Verbose
Update-Database -ConfigurationTypeName ConfigMigrationBookingContext -StartUpProjectName apcurium.MK.Web -ConnectionStringName MKWeb -Verbose

Use these commands to create new migration files when you make changes to the code/model and you want to synchronize the changes to the data model (SQL Server Database):

add-migration Booking_ARRO_0973 -ConfigurationTypeName apcurium.MK.Booking.Migrations.ConfigMigrationBookingContext -StartUpProject apcurium.MK.Web -ConnectionStringName MKWeb
add-migration Configuration_ARRO_0973 -ConfigurationTypeName apcurium.MK.Common.Migrations.ConfigMigrationConfigurationContext -StartUpProject apcurium.MK.Web -ConnectionStringName MKWeb

** don't forget to run the update-database commands after you create new migrations to apply the changes to your own database!

Previously apcurium were using a different naming convention, going forward, if a migration corresponds to a specific ARRO defect, use it in the migration name.

There are two DBContext defined in the solution:
BookingDbContext
and
ConfigurationDbContext

That's why there are two commands to issue when updating the database, or creating new migrations.

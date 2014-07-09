#region

using System;
using System.Linq;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using apcurium.MK.Booking.Database;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.Sql.EventSourcing;
using Infrastructure.Sql.MessageLog;
using Microsoft.Win32;

#endregion

namespace DatabaseInitializer.Sql
{
    public class DatabaseCreator
    {
        public void DropDatabase(string connStringMaster, string database)
        {
            var exists = "IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'" + database + "') ";

            DatabaseHelper.ExecuteNonQuery(connStringMaster,
                exists + "ALTER DATABASE [" + database + "] SET OFFLINE WITH ROLLBACK IMMEDIATE");

            DatabaseHelper.ExecuteNonQuery(connStringMaster, exists + "DROP DATABASE [" + database + "]");
        }




        public bool IsMirroringSet(string connStringMaster, string companyName)
        {

            var isMirrored = "SELECT mirroring_role     FROM sys.database_mirroring     WHERE DB_NAME(database_id) = N'" + companyName + "'";

            var result = DatabaseHelper.ExecuteNullableScalarQuery<byte>(connStringMaster, isMirrored);

            return result.HasValue && (result.Value == 1);
        }

        public void InitMirroring(string connStringMaster, string companyName)
        {
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"ALTER DATABASE {0} SET AUTO_CLOSE OFF", companyName));
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"ALTER DATABASE {0} SET RECOVERY FULL", companyName));
        }

        public void SetMirroringPartner(string connStringMaster, string companyName, string partner)
        {
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"ALTER DATABASE  {0} SET PARTNER='{1}'", companyName,partner));          
        }

        public  void CompleteMirroring(string connStringMaster, string companyName, string partner, string witness)
        {
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"ALTER DATABASE {0} SET PARTNER='{1}'", companyName, partner));
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"ALTER DATABASE {0} SET WITNESS='{1}'", companyName,witness));
          
        }

        
        
                    


        public void BackupDatabase(string connStringMaster, string backupFolder, string databaseName)
        {
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"BACKUP DATABASE {0} TO DISK='{1}\{0}.bak'  WITH FORMAT", databaseName, backupFolder));
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"BACKUP LOG {0} TO DISK='{1}\{0}_log.bak'  WITH FORMAT", databaseName, backupFolder));
        }

        public void RestoreToDeleteDatabase(string connStringMaster, string backupFolder, string databaseName)
        {
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"RESTORE DATABASE {0} FROM DISK = '{1}\{0}.bak'  WITH REPLACE, RECOVERY", databaseName, backupFolder));
        }
        public void RestoreDatabase(string connStringMaster, string backupFolder, string databaseName)
        {
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"RESTORE DATABASE {0} FROM DISK = '{1}\{0}.bak'  WITH REPLACE, NORECOVERY", databaseName, backupFolder));
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"RESTORE LOG {0} FROM DISK = '{1}\{0}_log.bak'  WITH REPLACE, NORECOVERY", databaseName, backupFolder));
        }

        

        public void TurnOffMirroring(string connStringMaster, string companyName)
        {
            var turnOff = string.Format("ALTER DATABASE {0} SET PARTNER OFF", companyName);
            DatabaseHelper.ExecuteNonQuery(connStringMaster, turnOff);
        }

        public bool DatabaseExists(string connStringMaster, string companyName)
        {
            var exists = "SELECT count(*) FROM sys.databases WHERE name = N'" + companyName + "'";

            var result = DatabaseHelper.ExecuteScalarQuery<int>(connStringMaster, exists);

            return result > 0;
        }

        public string RenameDatabase(string connectionString, string databaseName)
        {
            var exists = "IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'" + databaseName + "') ";

            DatabaseHelper.ExecuteNonQuery(connectionString,
                exists + "ALTER DATABASE [" + databaseName + "] SET OFFLINE WITH ROLLBACK IMMEDIATE");

            DatabaseHelper.ExecuteNonQuery(connectionString, exists + "ALTER DATABASE [" + databaseName + "] SET ONLINE");

            var oldSuffixe = DatabaseHelper.ExecuteScalarQuery<string>(connectionString, "DECLARE @DATABASE_ID int " +
                                                                                         "SET @DATABASE_ID = (SELECT database_id FROM master.sys.databases where name='" +
                                                                                         databaseName + "') " +
                                                                                         "SELECT SUBSTRING( name, (CHARINDEX(N'" +
                                                                                         databaseName +
                                                                                         "', LOWER(name)) + " +
                                                                                         databaseName.Length +
                                                                                         "),  LEN(name)) " +
                                                                                         "FROM master.sys.master_files " +
                                                                                         "WHERE database_id = @DATABASE_ID AND file_id = 1");

            var newName = databaseName + oldSuffixe;

            DatabaseHelper.ExecuteNonQuery(connectionString,
                exists + "ALTER DATABASE [" + databaseName + "] MODIFY NAME = [" + newName + "]");

            return newName;
        }

        public void CreateDatabase(string connectionString, string databaseName, string instanceName)
        {
            var exists = "IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'" + databaseName + "') ";

            DatabaseHelper.ExecuteNonQuery(connectionString,
                exists + "ALTER DATABASE [" + databaseName + "] SET OFFLINE WITH ROLLBACK IMMEDIATE");

            DatabaseHelper.ExecuteNonQuery(connectionString, exists + "ALTER DATABASE [" + databaseName + "] SET ONLINE");

            DatabaseHelper.ExecuteNonQuery(connectionString, exists + "DROP DATABASE [" + databaseName + "]");


            DatabaseHelper.ExecuteNonQuery(connectionString, string.Format("CREATE DATABASE [" + databaseName + "] " +
                                                                           "ON " +
                                                                           "( NAME = {3}_{2}, FILENAME = '{0}\\{3}_{2}.mdf' ) " +
                                                                           "LOG ON " +
                                                                           "( NAME = {3}_{2}_log, FILENAME = '{1}\\{3}_{2}.ldf' ) ",
                GetDataPath(instanceName),
                GetLogPath(instanceName),
                DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss"),
                databaseName));
        }

        public void AddUserAndRighst(string masterConnString, string dbConnString, string user, string dbName)
        {
            AddLoginToSqlServer(masterConnString, user);
            AddUserToDatabase(dbConnString, user, dbName);
            AddReadWriteRigthsToUserForADatabase(dbConnString, user, dbName);
        }

        private void AddLoginToSqlServer(string connectionString, string user)
        {
            DatabaseHelper.ExecuteNonQuery(connectionString,
                "IF NOT EXISTS (SELECT * FROM sys.server_principals where name = '" + user + "') CREATE LOGIN [" + user +
                "] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]");
        }

        private void AddUserToDatabase(string connectionString, string user, string dbName)
        {
            DatabaseHelper.ExecuteNonQuery(connectionString,
                "IF NOT EXISTS (SELECT dp.name FROM [" + dbName + "].sys.database_principals dp JOIN [" + dbName +
                "].sys.server_principals sp ON dp.sid = sp.sid WHERE sp.name = '" + user + "') CREATE USER [" + user +
                "] FOR LOGIN [" + user + "]");
            DatabaseHelper.ExecuteNonQuery(connectionString, "EXEC sp_addrolemember N'db_datareader', N'" + user + "'");
            DatabaseHelper.ExecuteNonQuery(connectionString, "EXEC sp_addrolemember N'db_datawriter', N'" + user + "'");
        }

        private void AddReadWriteRigthsToUserForADatabase(string connectionString, string user, string dbName)
        {
            var usernameForDb = DatabaseHelper.ExecuteScalarQuery<string>(connectionString,
                "SELECT dp.name FROM [" + dbName + "].sys.database_principals dp JOIN [" +
                dbName + "].sys.server_principals sp ON dp.sid = sp.sid WHERE sp.name = '" +
                user + "'");
            if (usernameForDb != "dbo")
            {
                DatabaseHelper.ExecuteNonQuery(connectionString,
                    "EXEC sp_addrolemember N'db_owner', N'" + usernameForDb + "'");
            }
        }

        public void FixUnorderedEvents(string connString)
        {
            Console.WriteLine("Checking for events in invalid order");

            string unorderedEvents = ";WITH cte AS ";
            unorderedEvents += "( ";
            unorderedEvents += "SELECT *, ROW_NUMBER() OVER (PARTITION BY AggregateId ORDER BY [EventDate] ASC) AS rn ";
            unorderedEvents += "FROM [Events].[Events]  ";
            unorderedEvents += ") ";
            unorderedEvents += "SELECT * ";
            unorderedEvents += "FROM cte ";
            unorderedEvents += "WHERE rn = 1 and Version > 0 ";
            unorderedEvents += "order by [EventDate] ";


            var invalidAggregateId = DatabaseHelper.ExecuteListQuery<Guid>(connString, unorderedEvents);

            if (invalidAggregateId.Any())
            {
                Console.WriteLine("Found " + invalidAggregateId.Count().ToString() + " events in invalid order");

                string fixEvents = "update [Events].[Events] set EventDate =  DATEADD(MINUTE, [Version] , (select top 1 EventDate from [Events].[Events] where AggregateId='{0}')) where AggregateId='{0}'";

                foreach (var invalidId in invalidAggregateId)
                {
                    DatabaseHelper.ExecuteNonQuery(connString, string.Format(fixEvents, invalidId.ToString()));
                }
            }
            else
            {
                Console.WriteLine("No event in invalid order");
            }




        }



        public void CopyEventsAndCacheTables(string connString, string oldDatabase, string newDatabase)
        {
            var queryForEvents =
                string.Format(
                    "INSERT INTO [{0}].[Events].[Events]([AggregateId] ,[AggregateType] ,[Version] ,[Payload] ,[CorrelationId], [EventType], [EventDate]) " +
                    "SELECT [AggregateId] ,[AggregateType] ,[Version] ,[Payload] ,[CorrelationId], [EventType], [EventDate] " +
                    "FROM [{1}].[Events].[Events] ", newDatabase, oldDatabase);

            DatabaseHelper.ExecuteNonQuery(connString, queryForEvents);

            var queryForCache = string.Format("INSERT INTO [{0}].[Cache].[Items]([Key],[Value],[ExpiresAt]) " +
                                              "SELECT [Key],[Value],[ExpiresAt] " +
                                              "FROM [{1}].[Cache].[Items] ", newDatabase, oldDatabase);

            DatabaseHelper.ExecuteNonQuery(connString, queryForCache);
        }

        public void CopyAppStartUpLogTable(string connString, string oldDatabase, string newDatabase)
        {
            var query =
                string.Format(
                    "INSERT INTO [{0}].[Booking].[AppStartUpLogDetail]([UserId] ,[DateOccured] ,[ApplicationVersion] ,[Platform] ,[PlatformDetails], [ServerVersion]) " +
                    "SELECT [UserId] ,[DateOccured] ,[ApplicationVersion] ,[Platform] ,[PlatformDetails], [ServerVersion] " +
                    "FROM [{1}].[Booking].[AppStartUpLogDetail] ", newDatabase, oldDatabase);

            try
            {
                DatabaseHelper.ExecuteNonQuery(connString, query);
            }
            catch (Exception)
            {
                // Ignore possible exceptions. Most probable case is trying to copy from source DB without this table
            }
        }

        private string GetLogPath(string instanceName)
        {
            return string.Format("{0}\\{1}", GetSqlRootPath(instanceName), "Log");
        }

        private string GetDataPath(string instanceName)
        {
            return string.Format("{0}\\{1}", GetSqlRootPath(instanceName), "Data");
        }

        private string GetSqlRootPath(string instanceName)
        {
            var key = Registry.LocalMachine;
            var subkey =
                key.OpenSubKey(string.Format("SOFTWARE\\Microsoft\\Microsoft SQL Server\\{0}\\Setup", instanceName));
            if (subkey == null)
            {
                throw new Exception(
                    "Can't retrieve the Data directory, did you specify the right instance name paremeter ?");
            }
            var value = subkey.GetValue("SQLDataRoot", string.Empty);
            return value.ToString();
        }

        public void CreateSchemas(ConnectionStringSettings connectionString)
        {
            Database.SetInitializer<EventStoreDbContext>(null);
            Database.SetInitializer<MessageLogDbContext>(null);
            Database.SetInitializer<BookingDbContext>(null);
            Database.SetInitializer<ConfigurationDbContext>(null);
            Database.SetInitializer<CachingDbContext>(null);

            var contexts = new DbContext[]
            {
                new CachingDbContext(connectionString.ConnectionString),
                new ConfigurationDbContext(connectionString.ConnectionString),
                new EventStoreDbContext(connectionString.ConnectionString),
                new MessageLogDbContext(connectionString.ConnectionString),
                new BookingDbContext(connectionString.ConnectionString)
            };

            try
            {
                foreach (var context in contexts)
                {
                    var adapter = (IObjectContextAdapter)context;

                    var script = adapter.ObjectContext.CreateDatabaseScript();

                    context.Database.ExecuteSqlCommand(script);

                    context.Dispose();
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                //TODO trouver un moyen plus sexy
            }
        }





        
    }
}
#region

using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;
using Infrastructure.Sql.EventSourcing;
using Infrastructure.Sql.MessageLog;
using log4net;
using Microsoft.Win32;

#endregion

namespace DatabaseInitializer.Sql
{
    public class DatabaseCreator
    {
        ILog _logger = LogManager.GetLogger("DatabaseInitializer");
        
        public void DropDatabase(string connStringMaster, string database, bool setoffline = true)
        {
            var exists = "IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'" + database + "') ";

            if (setoffline)
            {
                DatabaseHelper.ExecuteNonQuery(connStringMaster,
                    exists + "ALTER DATABASE [" + database + "] SET OFFLINE WITH ROLLBACK IMMEDIATE");
            }
            DatabaseHelper.ExecuteNonQuery(connStringMaster, exists + "DROP DATABASE [" + database + "]");
        }

        public void DropSchema(string connString, string databaseName)
        {
            string procedureName = "MkDropSchema_" + databaseName;
            DatabaseHelper.ExecuteNonQuery(connString, "IF OBJECT_ID('" + procedureName + "') IS NOT NULL DROP PROCEDURE " + procedureName);
            string dropTablesCreateProcSql = @"
                CREATE PROCEDURE " + procedureName + @" AS
                BEGIN 
                    DECLARE @Sql NVARCHAR(500) DECLARE @Cursor CURSOR;

                    SET @Cursor = CURSOR FAST_FORWARD FOR
                        SELECT DISTINCT sql = 'ALTER TABLE [' + tc2.TABLE_SCHEMA + '].[' + tc2.TABLE_NAME + '] DROP [' + rc1.CONSTRAINT_NAME + ']'
                        FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc1
                        LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc2 ON tc2.CONSTRAINT_NAME =rc1.CONSTRAINT_NAME
                        WHERE rc1.CONSTRAINT_CATALOG = '" + databaseName + @"';

                    OPEN @Cursor FETCH NEXT FROM @Cursor INTO @Sql;

                    WHILE (@@FETCH_STATUS = 0)
                    BEGIN
                        Exec SP_EXECUTESQL @Sql
                        FETCH NEXT FROM @Cursor INTO @Sql
                    END

                    CLOSE @Cursor DEALLOCATE @Cursor;

                    EXEC sp_MSForEachTable 'DROP TABLE ?';
                END
                ";
            DatabaseHelper.ExecuteNonQuery(connString, dropTablesCreateProcSql);
            DatabaseHelper.ExecuteNonQuery(connString, "EXEC " + procedureName);
            DatabaseHelper.ExecuteNonQuery(connString, "DROP PROCEDURE " + procedureName);
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
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"ALTER DATABASE {0} SET PARTNER='{1}'", companyName,partner));          
        }

        public  void CompleteMirroring(string connStringMaster, string companyName, string partner, string witness)
        {
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"ALTER DATABASE {0} SET PARTNER='{1}'", companyName, partner));
            if (witness.HasValue())
            {
                DatabaseHelper.ExecuteNonQuery(connStringMaster,
                    string.Format(@"ALTER DATABASE {0} SET WITNESS='{1}'", companyName, witness));
            }
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
            // Ensuring that nothing is connected to the database.
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"RESTORE DATABASE {0} FROM DISK = '{1}\{0}.bak'  WITH REPLACE, NORECOVERY", databaseName, backupFolder));
            DatabaseHelper.ExecuteNonQuery(connStringMaster, string.Format(@"RESTORE LOG {0} FROM DISK = '{1}\{0}_log.bak'  WITH REPLACE, NORECOVERY", databaseName, backupFolder));
        }

        public void TurnOffMirroring(string connStringMaster, string companyName)
        {
            // Disabling Mirroring
            var setMirroringOff = string.Format("ALTER DATABASE {0} SET PARTNER OFF ", companyName);
            DatabaseHelper.ExecuteNonQuery(connStringMaster, setMirroringOff);
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

        public void RenameDatabase(string connectionString, string oldName, string newName)
        {
            var exists = "IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'" + oldName + "') ";

            DatabaseHelper.ExecuteNonQuery(connectionString,
                exists + "ALTER DATABASE [" + oldName + "] SET OFFLINE WITH ROLLBACK IMMEDIATE");

            DatabaseHelper.ExecuteNonQuery(connectionString, exists + "ALTER DATABASE [" + oldName + "] SET ONLINE");

            DatabaseHelper.ExecuteNonQuery(connectionString,
                exists + "ALTER DATABASE [" + oldName + "] MODIFY NAME = [" + newName + "]");
        }

        public void CreateDatabase(string connectionString, string databaseName, string sqlDirectory)
        {
            var exists = "IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'" + databaseName + "') ";

            DatabaseHelper.ExecuteNonQuery(connectionString,
                exists + "ALTER DATABASE [" + databaseName + "] SET OFFLINE WITH ROLLBACK IMMEDIATE");

            DatabaseHelper.ExecuteNonQuery(connectionString, exists + "ALTER DATABASE [" + databaseName + "] SET ONLINE");

            DatabaseHelper.ExecuteNonQuery(connectionString, exists + "DROP DATABASE [" + databaseName + "]");


            var dataPath = Path.Combine(sqlDirectory, "DATA");
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
           
            var logPath = Path.Combine(sqlDirectory, "Log");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            DatabaseHelper.ExecuteNonQuery(connectionString, string.Format("CREATE DATABASE [" + databaseName + "] " +
                                                                           "ON " +
                                                                           "( NAME = {3}_{2}, FILENAME = '{0}\\{3}_{2}.mdf' ) " +
                                                                           "LOG ON " +
                                                                           "( NAME = {3}_{2}_log, FILENAME = '{1}\\{3}_{2}.ldf' ) ",
                dataPath,
                logPath,
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
            unorderedEvents += "SELECT *, ROW_NUMBER() OVER (PARTITION BY AggregateId ORDER BY [EventDate], [VERSION] ASC) AS rn ";
            unorderedEvents += "FROM [Events].[Events]  ";
            unorderedEvents += ") ";
            unorderedEvents += "SELECT [AggregateId] ";
            unorderedEvents += "FROM cte ";
            unorderedEvents += "WHERE (rn - [Version])  > 1 ";
            unorderedEvents += "group by [AggregateId] ";


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

        public void CreateIndexes(string connString, string newDatabase)
        {
            var createIndexForEventDate = string.Format("IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'EventDateVersionIdx' AND object_id = OBJECT_ID('[{0}].[Events].[Events]')) " +
                                                 "CREATE NONCLUSTERED INDEX [EventDateVersionIdx] ON [{0}].[Events].[Events] " +
                                                 "([EventDate] ASC, [Version] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, " +
                                                 "DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)", newDatabase);

            DatabaseHelper.ExecuteNonQuery(connString, createIndexForEventDate);

            var rebuildEventDateIndex = string.Format("ALTER INDEX [EventDateVersionIdx] ON [{0}].[Events].[Events] REBUILD PARTITION = ALL WITH (PAD_INDEX = OFF, " +
                                             "STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);", newDatabase);

            DatabaseHelper.ExecuteNonQuery(connString, rebuildEventDateIndex);

            var createIndexForOrderVehiclePosition = string.Format("IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'OrderIdIdx' AND object_id = OBJECT_ID('[{0}].[Booking].[OrderVehiclePositionDetail]')) " +
                                                 "CREATE NONCLUSTERED INDEX [OrderIdIdx] ON [{0}].[Booking].[OrderVehiclePositionDetail] " +
                                                 "([OrderId] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, " +
                                                 "DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)", newDatabase);

            DatabaseHelper.ExecuteNonQuery(connString, createIndexForOrderVehiclePosition);

            var createIndexForPromoIdUsage = string.Format("IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'PromoIdIdx' AND object_id = OBJECT_ID('[{0}].[Booking].[PromotionUsageDetail]')) " +
                                                 "CREATE NONCLUSTERED INDEX [PromoIdIdx] ON [{0}].[Booking].[PromotionUsageDetail] " +
                                                 "([PromoId] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, " +
                                                 "DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)", newDatabase);

            DatabaseHelper.ExecuteNonQuery(connString, createIndexForPromoIdUsage);
        }

        public DateTime? CopyEventsAndCacheTables(string connString, string oldDatabase, string newDatabase)
        {
            const int pageSize = 100000;

            //get the last events from the new database
            var lastProcessedEventTime = DatabaseHelper.ExecuteScalarQuery<DateTime?>(connString, string.Format("Select max([EventDate]) from [{0}].[Events].[Events]", newDatabase));

            var sqlDateTime = (DateTime)(lastProcessedEventTime ?? SqlDateTime.MinValue);

            Console.WriteLine("Counting number of events");

            var start = DateTime.Now;


            var queryNumberEvents = string.Format(
                @"Select Count(1)
                FROM [{0}].[Events].[Events]
                WHERE [EventType] <> 'apcurium.MK.Booking.Events.OrderVehiclePositionChanged' AND [EventDate] > '{1}'", oldDatabase, sqlDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            var nbEvents = DatabaseHelper.ExecuteScalarQuery<int>(connString, queryNumberEvents, 3600);

            Console.WriteLine("Original database has {0} events to copy (Duration: {1}) ", nbEvents, (DateTime.Now - start).TotalSeconds);
            
            if (nbEvents > 0)
            {
                var startRow = 0;

                start = DateTime.Now;

                const string queryBase =
                    @"INSERT INTO [{0}].[Events].[Events] ([AggregateId] ,[AggregateType] ,[Version] ,[Payload] ,[CorrelationId], [EventType], [EventDate]) " +
                    "SELECT item.AggregateId as AggregateId, item.AggregateType as AggregateType, item.Version as Version, item.Payload as Payload, item.CorrelationId as CorrelationId, item.EventType as EventType, item.EventDate as EventDate " +
                    "FROM (SELECT [AggregateId],[AggregateType],[Version],[Payload],[CorrelationId],[EventType],[EventDate], ROW_NUMBER() OVER(ORDER BY [EventDate]) as rownumber " +
                        "FROM [{1}].[Events].[Events] " +
                        "WHERE [EventType] <> 'apcurium.MK.Booking.Events.OrderVehiclePositionChanged' AND [EventDate] > '{2}') as item " +// delete OrderVehiclePositionChanged events
                    "WHERE rownumber >= {3} AND rownumber <= {4}";

                while (nbEvents > startRow)
                {
                    var endRow = startRow + pageSize > nbEvents
                        ? nbEvents
                        : startRow + pageSize;

                    startRow++;

                    var queryForEvents = string.Format(queryBase, newDatabase, oldDatabase, sqlDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), startRow, endRow);

                    Console.WriteLine("Copying events from row {0} to row {1}: (Timeout: 3600 seconds)", startRow, endRow);
                    DatabaseHelper.ExecuteNonQuery(connString, queryForEvents, 3600);

                    startRow = endRow;
                }

                Console.WriteLine("Finished copying events (Duration: {0})", (DateTime.Now - start).TotalSeconds);
            }
            

            // copy cache table except the static data
            var queryForCache = string.Format("INSERT INTO [{0}].[Cache].[Items]([Key],[Value],[ExpiresAt]) " +
                                              "SELECT [Key],[Value],[ExpiresAt] " +
                                              "FROM [{1}].[Cache].[Items] WHERE [Key] <> 'IBS.StaticData'", newDatabase, oldDatabase);

            DatabaseHelper.ExecuteNonQuery(connString, string.Format("TRUNCATE Table [{0}].[Cache].[Items]", newDatabase));
            DatabaseHelper.ExecuteNonQuery(connString, queryForCache);

            return lastProcessedEventTime;
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
                DatabaseHelper.ExecuteNonQuery(connString, string.Format("TRUNCATE Table [{0}].[Booking].[AppStartUpLogDetail]", newDatabase));
                DatabaseHelper.ExecuteNonQuery(connString, query);
            }
            catch (Exception)
            {
                // Ignore possible exceptions. Most probable case is trying to copy from source DB without this table
            }
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
            catch(Exception e)
            {
                _logger.Error("Error during Database Create Schema", e);
            }
        }
    }
}
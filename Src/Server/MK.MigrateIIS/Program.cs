using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Web.Administration;

namespace MK.MigrateIIS
{
    class Program
    {
        private static ServerManager _iisManager;

        private static void Main(string[] args)
        {
            try
            {
                _iisManager = new ServerManager();
                Console.WriteLine("[E]xport or [I]mport IIS Virtual Directories (see app.config file for import/export path) ?");
                var importOrExport = Console.ReadLine().ToUpperInvariant();

                if ("E".Equals(importOrExport))
                {
                    var exportPath = System.Configuration.ConfigurationManager.AppSettings["ExportDirectory"];
                    Console.WriteLine("Exporting to " + exportPath);
                    Export(exportPath);
                }

                if ("I".Equals(importOrExport))
                {
                    var importPath = System.Configuration.ConfigurationManager.AppSettings["ExportDirectory"];
                    Import(importPath);
                }
                Console.WriteLine("***********Success*****************");
            }
            catch (Exception exe)
            {
                Console.WriteLine("Error : ");
                Console.WriteLine(exe.Message);
                Console.WriteLine(exe.StackTrace);
            }
            finally
            {
                Console.ReadLine();
            }
        }

        private static void Export(string exportPath)
        {
            Console.WriteLine("Reading IIS Medata, list all applications under Default Web Site ");
            var website = _iisManager.Sites["Default Web Site"];
            foreach (var webApp in website.Applications.Where(x => x.Path != "/").ToList())
            {
                Console.WriteLine("Exporting application " + webApp.Path);
                var originalPath = webApp.VirtualDirectories["/"].PhysicalPath;
                var exportCompanyPath = Path.Combine(exportPath, webApp.Path.Substring(1, webApp.Path.Length - 1), "Current\\");
                CopyFiles(originalPath, exportCompanyPath);
            }
        }

        private static void Import(string importPath)
        {
            var website = _iisManager.Sites["TaxiHailSites"];
            Console.WriteLine("Importing webapplications from " + importPath);
            foreach (var dirPath in Directory.GetDirectories(importPath, "*", SearchOption.AllDirectories))
            {
                var companyName = dirPath.Substring(dirPath.LastIndexOf("\\", StringComparison.Ordinal) + 1);
                var appPool = _iisManager.ApplicationPools.FirstOrDefault(x => x.Name == companyName);
                if (appPool == null)
                {
                    Console.WriteLine("Creating a new app pool for " + companyName);
                    //create a new one
                    appPool = _iisManager.ApplicationPools.Add(companyName);
                    appPool.ManagedRuntimeVersion = "v4.0";
                    Thread.Sleep(2000);
                }
                
                //if (appPool.State == ObjectState.Started) appPool.Stop();

                Console.WriteLine("Creating/modifying virtual directory for " + companyName);
                var webApp = website.Applications.FirstOrDefault(x => x.Path == "/" + companyName);
                if (webApp != null)
                {
                    webApp.VirtualDirectories["/"].PhysicalPath = Path.Combine(dirPath, "Current");
                }
                else
                {
                    webApp = website.Applications.Add("/" + companyName, Path.Combine(dirPath, "Current"));
                    webApp.ApplicationPoolName = companyName;
                }

                _iisManager.CommitChanges();
                _iisManager = new ServerManager();
                website = _iisManager.Sites["TaxiHailSites"];
                Thread.Sleep(2000);

                Console.WriteLine("Configuring database access for " + companyName);
                var connectionString = new ConnectionStringSettings("MkWeb", string.Format("Data Source=.;Initial Catalog={0};Integrated Security=True; MultipleActiveResultSets=True", companyName));
                var connStringMaster = connectionString.ConnectionString.Replace(companyName, "master");
                AddUserAndRighst(connStringMaster, connectionString.ConnectionString, "IIS APPPOOL\\" + companyName, companyName);
            }
        }

        private static void CopyFiles(string source, string target)
        {
            Console.WriteLine("CopyFiles " + source + " => " + target);
            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }
            Directory.CreateDirectory(target);

            foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, target));

            foreach (var newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories).Where(x => !x.Contains("logfile.txt")))
                File.Copy(newPath, newPath.Replace(source, target));
        }

        public static void AddUserAndRighst(string masterConnString, string dbConnString, string user, string dbName)
        {
            AddLoginToSqlServer(masterConnString, user);
            AddUserToDatabase(dbConnString, user, dbName);
            AddReadWriteRigthsToUserForADatabase(dbConnString, user, dbName);
        }

        private static void AddLoginToSqlServer(string connectionString, string user)
        {
            DatabaseHelper.ExecuteNonQuery(connectionString,
                            "IF NOT EXISTS (SELECT * FROM sys.server_principals where name = '" + user + "') CREATE LOGIN [" + user + "] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]");
        }

        private static void AddUserToDatabase(string connectionString, string user, string dbName)
        {
            DatabaseHelper.ExecuteNonQuery(connectionString, "IF NOT EXISTS (SELECT dp.name FROM [" + dbName + "].sys.database_principals dp JOIN [" + dbName + "].sys.server_principals sp ON dp.sid = sp.sid WHERE sp.name = '" + user + "') CREATE USER [" + user + "] FOR LOGIN [" + user + "]");
            DatabaseHelper.ExecuteNonQuery(connectionString, "EXEC sp_addrolemember N'db_datareader', N'" + user + "'");
            DatabaseHelper.ExecuteNonQuery(connectionString, "EXEC sp_addrolemember N'db_datawriter', N'" + user + "'");
        }

        private static void AddReadWriteRigthsToUserForADatabase(string connectionString, string user, string dbName)
        {

            var usernameForDb = DatabaseHelper.ExecuteScalarQuery(connectionString,
                                              "SELECT dp.name FROM [" + dbName + "].sys.database_principals dp JOIN [" +
                                              dbName + "].sys.server_principals sp ON dp.sid = sp.sid WHERE sp.name = '" +
                                              user + "'");
            if (usernameForDb != "dbo")
            {
                DatabaseHelper.ExecuteNonQuery(connectionString, "EXEC sp_addrolemember N'db_owner', N'" + usernameForDb + "'");
            }
        }
    }
}

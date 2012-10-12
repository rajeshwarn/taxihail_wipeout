using System;
using System.IO;
using System.Reflection;
using DatabaseInitializer.Services;
using DatabaseInitializer.Sql;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using Newtonsoft.Json.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common.Configuration.Impl;
using log4net;
using log4net.Config;

namespace DatabaseInitializer
{
    using System.Configuration;

    public class Program
    {
        public static int  Main(string[] args)
        {
            var loggger = LogManager.GetLogger("DatabaseInitializer");
            try
            {
                var companyName = "MKWebDev";
                if (args.Length > 0)
                {
                    companyName = args[0];
                }

                var connectionString = new ConnectionStringSettings("MkWeb",
                                                                    "Data Source=.;Initial Catalog=MKWebDev;Integrated Security=True; MultipleActiveResultSets=True");
                if (args.Length > 1)
                {
                    connectionString.ConnectionString = args[1];
                }

                var connStringMaster = connectionString.ConnectionString.Replace(companyName, "master");

                //Init or Update
                bool isUpdate;
                if (args.Length > 2)
                {
                    isUpdate = args[2].ToUpperInvariant() == "U";
                }
                else
                {
                    Console.WriteLine("[C]reate (drop existing) or [U]pdate database ?");
                    isUpdate = Console.ReadLine() == "U";
                }

                //SQL Instance name
                var sqlInstanceName = "MSSQL11.MSSQLSERVER";
                if (args.Length > 3)
                {
                    sqlInstanceName = args[3];
                }
                else
                {
                    Console.WriteLine("Sql Instance name ? Default is MSSQL11.MSSQLSERVER , 1- MSSQL10_50.MSSQLSERVER");
                    var userSqlInstance = Console.ReadLine();

                    sqlInstanceName = string.IsNullOrEmpty(userSqlInstance) ? sqlInstanceName : userSqlInstance == "1" ?"MSSQL10_50.MSSQLSERVER" : userSqlInstance;
                }

                var creatorDb = new DatabaseCreator();
                string oldDatabase = null;
                if (isUpdate)
                {
                    oldDatabase = creatorDb.RenameDatabase(connStringMaster, companyName);
                }

                creatorDb.CreateDatabase(connStringMaster, companyName, sqlInstanceName);
                creatorDb.CreateSchemas(connectionString);

                //add user for IIS IIS APPPOOL\MyCompany
                if (companyName != "MKWebDev")
                {
                    creatorDb.AddUserAndRighst(connStringMaster, connectionString.ConnectionString,
                                               "IIS APPPOOL\\" + companyName, companyName);
                }

                //Copy Domain Events
                if (isUpdate)
                {
                    creatorDb.CopyDomainEventFromOldToNewDatabase(connStringMaster, oldDatabase, companyName);
                }

                //Create settings
                var configurationManager = new
                    apcurium.MK.Common.Configuration.Impl.ConfigurationManager(
                    () => new ConfigurationDbContext(connectionString.ConnectionString));

                var jsonSettings = File.ReadAllText(Path.Combine(AssemblyDirectory, "Settings\\", companyName + ".json"));
                var objectSettings = JObject.Parse(jsonSettings);
                foreach (var token in objectSettings)
                {
                    configurationManager.SetSetting(token.Key, token.Value.ToString());
                }
                 

                //Init container
                var container = new UnityContainer();
                var module = new Module();
                module.Init(container, connectionString);

                if (isUpdate)
                {
                    //replay events
                    var replayService = container.Resolve<IEventsPlayBackService>();
                    replayService.ReplayAllEvents();

                }
                else
                {
                    


                    //Init data
                    var commandBus = container.Resolve<ICommandBus>();

                    var registerAccountCommand = new RegisterAccount
                                                     {
                                                         Id = Guid.NewGuid(),
                                                         AccountId = Guid.NewGuid(),
                                                         Email = "john@taxihail.com",
                                                         Name = "John Doe",
                                                         Phone = "5146543024",
                                                         Password = "password"
                                                     };

                    var confirmationToken = Guid.NewGuid();
                    registerAccountCommand.ConfimationToken = confirmationToken.ToString();

                    var accountWebServiceClient = container.Resolve<IAccountWebServiceClient>();
                    registerAccountCommand.IbsAccountId =
                        accountWebServiceClient.CreateAccount(registerAccountCommand.AccountId,
                                                              registerAccountCommand.Email,
                                                              string.Empty,
                                                              registerAccountCommand.Name,
                                                              registerAccountCommand.Phone);
                    commandBus.Send(registerAccountCommand);


                    commandBus.Send(new ConfirmAccount
                                        {
                                            AccountId = registerAccountCommand.AccountId,
                                            ConfimationToken = registerAccountCommand.ConfimationToken
                                        });
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message + " " +e.StackTrace);
                loggger.Fatal(e.Message, e);
                return -1;
            }
            return 0;
        }

        static public string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}

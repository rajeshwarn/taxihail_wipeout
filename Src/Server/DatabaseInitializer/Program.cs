#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using DatabaseInitializer.Services;
using DatabaseInitializer.Sql;
using Infrastructure.Messaging;
using log4net;
using Microsoft.Practices.Unity;
using Newtonsoft.Json.Linq;
using ConfigurationManager = apcurium.MK.Common.Configuration.Impl.ConfigurationManager;
using DeploymentServiceTools;
using ServiceStack.Text;
using RegisterAccount = apcurium.MK.Booking.Commands.RegisterAccount;

#endregion

namespace DatabaseInitializer
{
    public class Program
    {
        private static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private static string CurrentVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();                
            }
        }

        public static int Main(string[] args)
        {
            var logger = LogManager.GetLogger("DatabaseInitializer");
            try
            {
                Console.WriteLine("Creating/updating database for version {0}", CurrentVersion);

                var param = GetParamsFromArgs(args);
                var connectionString = new ConnectionStringSettings("MkWeb", param.MkWebConnectionString);
                
                Console.WriteLine("Working..." );

                var creatorDb = new DatabaseCreator();
                var isUpdate = creatorDb.DatabaseExists(param.MasterConnectionString , param.CompanyName);
                if (isUpdate)
                {
                    Console.WriteLine("Updating database...");
                    var mirrored = creatorDb.IsMirroringSet(param.MasterConnectionString, param.CompanyName);
                    if (mirrored)
                    {
                        Console.WriteLine("Turning off database mirroring...");
                        creatorDb.TurnOffMirroring(param.MasterConnectionString, param.CompanyName);
                        Console.WriteLine("Database mirroring turned off.");
                    }                    
                }
                
                string oldDatabase = null;
                var container = new UnityContainer();
                var module = new Module();
                module.Init(container, connectionString);
                
                var configurationManager = new ConfigurationManager
                    (() => new ConfigurationDbContext(connectionString.ConnectionString), container.Resolve<ILogger>());

                //for dev company, delete old database to prevent keeping too many databases
                if (param.CompanyName == "MKWebDev" && isUpdate)
                {
#if DEBUG
                    Console.WriteLine("Drop Existing Database? Y or N");
                    var shouldDrop = args.Length > 2 ? args[2] : Console.ReadLine();
                    if ("Y".Equals(shouldDrop, StringComparison.OrdinalIgnoreCase))
                    {
                        creatorDb.DropDatabase(param.MasterConnectionString, param.CompanyName);
                        isUpdate = false;
                    }
#endif
                }

                var appSettings = GetCombinedSettings(isUpdate ? configurationManager.GetSettings() : null, param.CompanyName);

                if (isUpdate)
                {                    
                    oldDatabase = creatorDb.RenameDatabase(param.MasterConnectionString, param.CompanyName);
                }
               
                creatorDb.CreateDatabase(param.MasterConnectionString, param.CompanyName, param.SqlInstanceName);
                creatorDb.CreateSchemas(connectionString);

                Console.WriteLine("Add user for IIS...");

                ////add user for IIS IIS APPPOOL\MyCompany
                if ((param.CompanyName != "MKWebDev") && (connectionString.ConnectionString.ToLower().Contains("integrated security=true")))
                {
                    creatorDb.AddUserAndRighst(param.MasterConnectionString, connectionString.ConnectionString,
                        "IIS APPPOOL\\" + param.CompanyName, param.CompanyName);
                }

                //Copy Domain Events
                if (isUpdate)
                {
                    creatorDb.CopyEventsAndCacheTables(param.MasterConnectionString, oldDatabase, param.CompanyName);
                    creatorDb.CopyAppStartUpLogTable(param.MasterConnectionString, oldDatabase, param.CompanyName);
                    creatorDb.FixUnorderedEvents(connectionString.ConnectionString);
                }

                //Init data
                var commandBus = container.Resolve<ICommandBus>();
                var companyIsCreated = container.Resolve<IEventsPlayBackService>().CountEvent("Company") > 0;

                if (!companyIsCreated)
                {
                    // Create Default company
                    commandBus.Send(new CreateCompany
                    {
                        CompanyId = AppConstants.CompanyId,
                        Id = Guid.NewGuid()
                    });
                }
               
                if (isUpdate)
                {
                    Console.WriteLine("Migrating events...");
                    //migrate events
                    var migrator = container.Resolve<IEventsMigrator>();

                    migrator.Do();
                }

                //Save settings so that next calls to referenceDataService has the IBS Url
                AddOrUpdateAppSettings(commandBus, appSettings);

                if (isUpdate)
                {
                    Console.WriteLine("Playing events...");

                    //replay events
                    var replayService = container.Resolve<IEventsPlayBackService>();
                    replayService.ReplayAllEvents();

                    var tariffs = new TariffDao(() => new BookingDbContext(connectionString.ConnectionString));
                    if (tariffs.GetAll().All(x => x.Type != (int)TariffType.Default))
                    {
                        // Default rate does not exist for this company 
                        CreateDefaultTariff(configurationManager, commandBus);
                    }

                    CreateDefaultRules(connectionString, commandBus);
                    Console.WriteLine("Done playing events...");

                    EnsureDefaultAccountsExists(connectionString, commandBus);
                }
                else
                {                    
                    // Create default rate for company
                    CreateDefaultTariff(configurationManager, commandBus);
                    CreateDefaultRules(connectionString, commandBus);

                    FetchingIbsDefaults(container, commandBus);

                    CreateDefaultAccounts(container, commandBus);

                    AddDefaultRatings(commandBus);
                }

                // Update vehicle types
                var vehicleTypes = new VehicleTypeDao(() => new BookingDbContext(connectionString.ConnectionString));
                if (!vehicleTypes.GetAll().Any())
                {
                    appSettings["Client.VehicleEstimateEnabled"] = "false";
                    AddOrUpdateAppSettings(commandBus, appSettings);
                    CreateDefaultVehicleTypes(container, commandBus);
                }

                if (isUpdate && !string.IsNullOrEmpty(param.BackupFolder))
                {
                    Console.WriteLine("Backup of old database...");
                    var backupFolder = Path.Combine(param.BackupFolder, param.CompanyName + DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss"));
                    Directory.CreateDirectory(backupFolder);
                    creatorDb.BackupDatabase(param.MasterConnectionString, backupFolder, oldDatabase );

                    Console.WriteLine("Dropping of old database...");
                    creatorDb.DropDatabase(param.MasterConnectionString, oldDatabase);                    
                }

                if (!string.IsNullOrEmpty( param.MirroringSharedFolder ))
                {
                    Console.WriteLine("MirroringSharedFolder :" + param.MirroringSharedFolder );
                    Console.WriteLine("Setting up mirroring...");
                    var backupFolder = Path.Combine(param.MirroringSharedFolder, param.CompanyName + DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss"));
                    Directory.CreateDirectory(backupFolder);
                    
                    creatorDb.InitMirroring(param.MasterConnectionString, param.CompanyName);
                    Console.WriteLine("Backup for mirroring...");
                    creatorDb.BackupDatabase(param.MasterConnectionString, backupFolder, param.CompanyName);

                    var mirrorDbExist = creatorDb.DatabaseExists(param.MirrorMasterConnectionString, param.CompanyName);
                    if ( mirrorDbExist )
                    {
                        Console.WriteLine("Deleting existing mirroring db...");
                        creatorDb.RestoreToDeleteDatabase(param.MirrorMasterConnectionString, backupFolder, param.CompanyName);
                        creatorDb.DropDatabase(param.MirrorMasterConnectionString, param.CompanyName);
                    }

                    Console.WriteLine("Restoring mirroring backup...");
                    creatorDb.RestoreDatabase(param.MirrorMasterConnectionString, backupFolder, param.CompanyName);
                    
                    creatorDb.SetMirroringPartner(param.MirrorMasterConnectionString, param.CompanyName, param.MirroringMirrorPartner );
                    creatorDb.CompleteMirroring(param.MasterConnectionString , param.CompanyName, param.MirroringPrincipalPartner, param.MirroringWitness );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
                logger.Fatal(e.Message, e);
                return 1;
            }
            return 0;
// ReSharper restore LocalizableElement
        }

        private static void AddDefaultRatings(ICommandBus commandBus)
        {
            commandBus.Send(new AddRatingType
            {
                CompanyId = AppConstants.CompanyId,
                Name = "Knowledgable driver",
                RatingTypeId = Guid.NewGuid()
            });
            commandBus.Send(new AddRatingType
            {
                CompanyId = AppConstants.CompanyId,
                Name = "Politeness",
                RatingTypeId = Guid.NewGuid()
            });
            commandBus.Send(new AddRatingType
            {
                CompanyId = AppConstants.CompanyId,
                Name = "Safety",
                RatingTypeId = Guid.NewGuid()
            });
        }

        private static void CreateDefaultAccounts(UnityContainer container, ICommandBus commandBus)
        {
            Console.WriteLine("Register normal account...");
            //Register normal account
            var registerAccountCommand = new RegisterAccount
            {
                Id = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                Email = "john@taxihail.com",
                Name = "John Doe",
                Phone = "6132875020",
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

            //Register admin account
            Console.WriteLine("Register admin account...");
            var registerAdminAccountCommand = new RegisterAccount
            {
                Id = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                Email = "taxihail@apcurium.com",
                Name = "Administrator",
                Phone = "6132875020",
                Password = "1l1k3B4n4n@", //Todo Make the admin portal customizable
                IsAdmin = true
            };

            var confirmationAdminToken = Guid.NewGuid();
            registerAdminAccountCommand.ConfimationToken = confirmationAdminToken.ToString();

            registerAdminAccountCommand.IbsAccountId =
                accountWebServiceClient.CreateAccount(registerAdminAccountCommand.AccountId,
                    registerAdminAccountCommand.Email,
                    string.Empty,
                    registerAdminAccountCommand.Name,
                    registerAdminAccountCommand.Phone);
            commandBus.Send(registerAdminAccountCommand);
            commandBus.Send(new AddRoleToUserAccount
            {
                AccountId = registerAdminAccountCommand.AccountId,
                RoleName = RoleName.SuperAdmin,
            });

            commandBus.Send(new ConfirmAccount
            {
                AccountId = registerAdminAccountCommand.AccountId,
                ConfimationToken = registerAdminAccountCommand.ConfimationToken
            });
        }

        private static DatabaseInitializerParams GetParamsFromArgs(string[] args)
        {
            var result = new DatabaseInitializerParams();

            Console.WriteLine("args : " + args.JoinBy(" "));
            if (args.Any() && !string.IsNullOrWhiteSpace(args[0]) && args[0].Trim().StartsWith("f:"))
            {
                var paramFile =  Path.Combine( AssemblyDirectory , args[0].Trim().Substring(2, args[0].Trim().Length - 2));
                Console.WriteLine("Reading param file : " + paramFile );
                if ( !File.Exists(paramFile ))
                {
                    throw new ApplicationException("Parameteres file cannot be found");
                }
                var paramFileContent = File.ReadAllText(paramFile);

                result = ServiceStack.Text.JsonSerializer.DeserializeFromString<DatabaseInitializerParams>(paramFileContent); 
            }
            else if (args.Length > 0)
            {
                result.CompanyName = args[0];
            }

            result.CompanyName = string.IsNullOrWhiteSpace(result.CompanyName) ? "MKWebDev" : result.CompanyName;

            //Sql instance name
            if (string.IsNullOrWhiteSpace(result.MkWebConnectionString) && (args.Length > 1))
            {
                result.SqlInstanceName = args[1];
            }
            else if (string.IsNullOrWhiteSpace(result.SqlInstanceName))
            {
                var sqlInstanceName = "MSSQL11.MSSQLSERVER";
                Console.WriteLine("Sql Instance name ? Default is MSSQL11.MSSQLSERVER , 1- MSSQL10_50.MSSQLSERVER, 2- MSSQL12.MSSQLSERVER");

                var userSqlInstance = Console.ReadLine();

                sqlInstanceName = string.IsNullOrEmpty(userSqlInstance)
                    ? sqlInstanceName
                    : userSqlInstance == "1"
                    ? "MSSQL10_50.MSSQLSERVER"
                    : userSqlInstance == "2"
                    ? "MSSQL12.MSSQLSERVER"
                    : sqlInstanceName;

                result.SqlInstanceName = sqlInstanceName;
            }
            
            //Company connection string
            if (string.IsNullOrWhiteSpace(result.MkWebConnectionString) && (args.Length > 3))
            {
                result.MkWebConnectionString = string.Format(args[3], result.CompanyName);
            }
            else if (string.IsNullOrWhiteSpace(result.MkWebConnectionString))
            {
                result.MkWebConnectionString = string.Format("Data Source=.;Initial Catalog={0};Integrated Security=True; MultipleActiveResultSets=True", result.CompanyName);
            }

            //Master connection string           
            if (string.IsNullOrWhiteSpace(result.MasterConnectionString) && (args.Length > 4))
            {
                result.MasterConnectionString = string.Format(args[4], result.CompanyName);
            }
            else if (string.IsNullOrWhiteSpace(result.MasterConnectionString))
            {
                result.MasterConnectionString = result.MkWebConnectionString.Replace(result.CompanyName, "master");
            }

            Console.WriteLine("Running database initializer using the following settings : ");

            var j = result.ToJson();
            Console.WriteLine(j);

            return result;
        }

        private static void FetchingIbsDefaults(UnityContainer container, ICommandBus commandBus)
        {
            var appSettings = new Dictionary<string, string>(); 
            Console.WriteLine("Calling ibs...");
            //Get default settings from IBS
            var referenceDataService = container.Resolve<IStaticDataWebServiceClient>();

            var defaultCompany = referenceDataService.GetCompaniesList()
                .FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value)
                                 ?? referenceDataService.GetCompaniesList().FirstOrDefault();

            if (defaultCompany != null)
            {
                appSettings["DefaultBookingSettings.ProviderId"] = defaultCompany.Id.ToString();

                var defaultvehicule =
                    referenceDataService.GetVehiclesList(defaultCompany)
                        .FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value) ??
                    referenceDataService.GetVehiclesList(defaultCompany).First();
                appSettings["DefaultBookingSettings.VehicleTypeId"] = defaultvehicule.Id.ToString();

                appSettings["DefaultBookingSettings.ChargeTypeId"] = ChargeTypes.PaymentInCar.Id.ToString();
            }

            //Save settings so that registerAccountCommand succeed
            AddOrUpdateAppSettings(commandBus, appSettings);         
        }

        private static void EnsureDefaultAccountsExists(ConnectionStringSettings connectionString, ICommandBus commandBus)
        {
            var accounts = new AccountDao(() => new BookingDbContext(connectionString.ConnectionString));
            var admin = accounts.FindByEmail("taxihail@apcurium.com");

            if (admin != null)
            {
                commandBus.Send(new AddRoleToUserAccount
                {
                    AccountId = admin.Id,
                    RoleName = RoleName.SuperAdmin,
                });


                commandBus.Send(new ConfirmAccount
                {
                    AccountId = admin.Id,
                    ConfimationToken = admin.ConfirmationToken
                });
            }

            var john = accounts.FindByEmail("john@taxihail.com");

            if (john != null)
            {
                var updateAccountCommand = new UpdateBookingSettings
                {
                    AccountId = john.Id,
                    Phone = "6132875020",
                    Name = "John Doe",
                    NumberOfTaxi = john.Settings.NumberOfTaxi,
                    ChargeTypeId = john.Settings.ChargeTypeId,
                    DefaultCreditCard = john.DefaultCreditCard,
                    DefaultTipPercent = john.DefaultTipPercent
                };
                commandBus.Send(updateAccountCommand);
            }
        }

        private static void CreateDefaultRules(ConnectionStringSettings connectionString, ICommandBus commandBus)
        {
            var rules = new RuleDao(() => new BookingDbContext(connectionString.ConnectionString));
            if (
                rules.GetAll()
                    .None(r => (r.Category == (int) RuleCategory.WarningRule) && (r.Type == (int) RuleType.Default)))
            {
                // Default rate does not exist for this company 
                commandBus.Send(new CreateRule
                {
                    Type = RuleType.Default,
                    Category = RuleCategory.WarningRule,
                    CompanyId = AppConstants.CompanyId,
                    AppliesToCurrentBooking = true,
                    AppliesToFutureBooking = true,
                    RuleId = Guid.NewGuid(),
                    IsActive = false,
                    Name = "Default",
                    Message = "Due to the current volume of calls, please note that pickup may be delayed.",
                    ZoneList = " "
                });
            }

            if (
                rules.GetAll()
                    .None(r => (r.Category == (int) RuleCategory.DisableRule) && (r.Type == (int) RuleType.Default)))
            {
                // Default rate does not exist for this company 
                commandBus.Send(new CreateRule
                {
                    Type = RuleType.Default,
                    Category = RuleCategory.DisableRule,
                    CompanyId = AppConstants.CompanyId,
                    AppliesToCurrentBooking = true,
                    AppliesToFutureBooking = true,
                    RuleId = Guid.NewGuid(),
                    Name = "Default",
                    Message = "Service is temporarily unavailable. Please call dispatch center for service.",
                });
            }
        }

        private static Dictionary<string, string> GetCombinedSettings(IDictionary<string, string> settingsInDb, string companyName )
        {            
            //Create settings
            var appSettings = new Dictionary<string, string>();
            var jsonSettings = File.ReadAllText(Path.Combine(AssemblyDirectory, "Settings\\Common.json"));
            var objectSettings = JObject.Parse(jsonSettings);

            Console.WriteLine("Loading settings...");
            
            foreach (var token in objectSettings)
            {
                appSettings[token.Key] = token.Value.ToString();
            }

            jsonSettings = File.ReadAllText(Path.Combine(AssemblyDirectory, "Settings\\", companyName + ".json"));
            objectSettings = JObject.Parse(jsonSettings);
            
            foreach (var token in objectSettings)
            {
                appSettings[token.Key] = token.Value.ToString();
            }

            if (settingsInDb != null)
            {
                settingsInDb.ForEach(setting => appSettings[setting.Key] = setting.Value);
            }

            return appSettings;
        }

        private static void AddOrUpdateAppSettings(ICommandBus commandBus, Dictionary<string, string> appSettings)
        {
            commandBus.Send(new AddOrUpdateAppSettings
            {
                AppSettings = appSettings,
                CompanyId = AppConstants.CompanyId
            });
        }

        private static void CreateDefaultTariff(IConfigurationManager configurationManager, ICommandBus commandBus)
        {
            var flatRate = configurationManager.GetSetting("Direction.FlateRate");
            var ratePerKm = configurationManager.GetSetting("Direction.RatePerKm");

            commandBus.Send(new CreateTariff
            {
                Type = TariffType.Default,
                KilometricRate = double.Parse(ratePerKm, CultureInfo.InvariantCulture),
                FlatRate = decimal.Parse(flatRate, CultureInfo.InvariantCulture),
                MarginOfError = 20,
                CompanyId = AppConstants.CompanyId,
                TariffId = Guid.NewGuid(),
            });
        }

        private static void CreateDefaultVehicleTypes(UnityContainer container, ICommandBus commandBus)
        {
            var referenceDataService = container.Resolve<ReferenceDataService>();
            var referenceData = (ReferenceData) referenceDataService.Get(new ReferenceDataRequest());

            foreach (var vehicle in referenceData.VehiclesList)
            {
                commandBus.Send(new AddUpdateVehicleType
                {
                    VehicleTypeId = Guid.NewGuid(),
                    Name = string.Format("{0}", vehicle.Display),
                    LogoName = "taxi",
                    ReferenceDataVehicleId = vehicle.Id ?? -1,
                    CompanyId = AppConstants.CompanyId
                });
            }
        }
    }
}
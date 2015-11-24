#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using DatabaseInitializer.Services;
using DatabaseInitializer.Sql;
using Infrastructure.Messaging;
using log4net;
using Microsoft.Practices.Unity;
using Microsoft.Web.Administration;
using MK.Common.Configuration;
using Newtonsoft.Json.Linq;
using DeploymentServiceTools;
using ServiceStack.Messaging.Rcon;
using ServiceStack.Text;
using RegisterAccount = apcurium.MK.Booking.Commands.RegisterAccount;

#endregion

namespace DatabaseInitializer
{
    public class Program
    {
        private const string LocalDevProjectName = "MKWebDev";

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
                Console.WriteLine("Started at: {0}", DateTime.Now);

                Console.WriteLine("Initializing...");
                var param = GetParamsFromArgs(args);

                Console.WriteLine("Working...");

                UnityContainer container;
                Module module;

                var creatorDb = new DatabaseCreator();
                IsUpdate = creatorDb.DatabaseExists(param.MasterConnectionString, param.CompanyName);
                IDictionary<string, string> appSettings;

                //for dev company, delete old database to prevent keeping too many databases
                if (param.CompanyName == LocalDevProjectName && IsUpdate)
                {
#if DEBUG
                    Console.WriteLine("Drop Existing Database? Y or N");
                    var shouldDrop = args.Length > 1 ? args[1] : Console.ReadLine();
                    if ("Y".Equals(shouldDrop, StringComparison.OrdinalIgnoreCase))
                    {
                        if (param.ReuseTemporaryDb)
                        {
                            creatorDb.DropSchema(param.MkWebConnectionString, param.CompanyName);
                        }
                        else
                        {
                            creatorDb.DropDatabase(param.MasterConnectionString, param.CompanyName);
                        }
                        IsUpdate = false;
                    }
#endif
                }

                if (IsUpdate)
                {
                    creatorDb.DeleteDeviceRegisteredEvents(param.MasterConnectionString, param.CompanyName);

                    UpdateSchema(param);

                    if (param.ReuseTemporaryDb)
                    {
                        // the idea behind reuse of temp db is that account doesn't have permission to rename db 
                        // so we instead we need to re-migrate from the temp db to the actual name
                        UpdateSchema(param);
                    }

                }
                else
                {
                    // if DBs are re-used then it should already be created
                    if (!param.ReuseTemporaryDb)
                    {
                        creatorDb.CreateDatabase(param.MasterConnectionString, param.CompanyName, param.SqlServerDirectory);
                    }
                    creatorDb.CreateSchemas(new ConnectionStringSettings("MkWeb", param.MkWebConnectionString));

                    UpdateSchema(param);

                    creatorDb.CreateIndexes(param.MasterConnectionString, param.CompanyName);

                    Console.WriteLine("Add user for IIS...");
                    if (param.MkWebConnectionString.ToLower().Contains("integrated security=true"))
                    {
                        creatorDb.AddUserAndRighst(param.MasterConnectionString, param.MkWebConnectionString,
                            "IIS APPPOOL\\" + param.AppPoolName, param.CompanyName);
                    }

                    SetupMirroring(param);
                }

                var connectionString = new ConnectionStringSettings("MkWeb", param.MkWebConnectionString);
                container = new UnityContainer();
                module = new Module();
                module.Init(container, connectionString, param.MkWebConnectionString);

                var serverSettings = container.Resolve<IServerSettings>();
                var commandBus = container.Resolve<ICommandBus>();

                Console.WriteLine("Checking Company Created...");
                CheckCompanyCreated(container, commandBus);

                if (!IsUpdate)
                {
                    appSettings = GetCompanySettings(param.CompanyName);

                    //Save settings so that next calls to referenceDataService has the IBS Url
                    AddOrUpdateAppSettings(commandBus, appSettings);

                    FetchingIbsDefaults(container, commandBus);

                    CreateDefaultAccounts(container, commandBus);
                }
                else
                {
                    EnsureSupportRoleIsSupported(connectionString, commandBus);
                    appSettings = serverSettings.GetSettings();
                }

				// If we are deploying to staging, regardless if we are updating or creating a new DB, create the apple test account.
				if (param.IsStaging)
				{
					CreateAppleTestAccountIfNeeded(container, commandBus);
				}

                Console.WriteLine("Checking Rules...");
                CheckandMigrateDefaultRules(connectionString, commandBus, appSettings);
                Console.WriteLine("Checking Default Account Settings ...");
                EnsureDefaultAccountsHasCorrectSettings(connectionString, commandBus);

                SetDefaultAdminSettings(serverSettings, appSettings);

                Console.WriteLine("Checking Default Tariff ...");
                CreateDefaultTariff(connectionString.ConnectionString, serverSettings, commandBus);

                Console.WriteLine("Checking Ratings ...");
                var ratingTypes = new RatingTypeDao(() => new BookingDbContext(connectionString.ConnectionString)).GetAll();
                if (!ratingTypes.Any())
                {
                    AddDefaultRatings(commandBus);
                }
                else
                {
                    UpdateRatings(commandBus, ratingTypes);
                }

                Console.WriteLine("Checking Vehicles Types ...");
                var vehicleTypeDao = new VehicleTypeDao(() => new BookingDbContext(connectionString.ConnectionString));
                var vehicleTypeDetails = vehicleTypeDao.GetAll();
                if (!vehicleTypeDetails.Any())
                {
                    appSettings["VehicleTypeSelectionEnabled"] = "false";
                    AddOrUpdateAppSettings(commandBus, appSettings);
                    CreateDefaultVehicleTypes(container, commandBus);
                }
                else
                {
                    CheckAndAddCapacity(vehicleTypeDetails, container, commandBus);
                }

                Console.WriteLine("Migration of Notification Settings ...");
                var configDao = new ConfigurationDao(() => new ConfigurationDbContext(connectionString.ConnectionString));
                if (configDao.GetNotificationSettings() == null)
                {
                    commandBus.Send(new AddOrUpdateNotificationSettings
                    {
                        CompanyId = AppConstants.CompanyId,
                        NotificationSettings = new NotificationSettings
                        {
                            Enabled = true,
                            BookingConfirmationEmail = true,
                            ConfirmPairingPush = true,
                            DriverAssignedPush = true,
                            NearbyTaxiPush = true,
                            UnpairingReminderPush = true,
                            PaymentConfirmationPush = true,
                            ReceiptEmail = true,
                            PromotionUnlockedEmail = true,
                            VehicleAtPickupPush = true,
                            PromotionUnlockedPush = true,
                            DriverBailedPush = true
                        }
                    });
                }

                Console.WriteLine("Migration of Payment Settings ...");

                MigratePaymentSettings(serverSettings, commandBus);

                EnsurePrivacyPolicyExists(connectionString, commandBus, serverSettings);

#if DEBUG
                if (IsUpdate)
                {
                    var iisManager = new ServerManager();
                    var appPool = iisManager.ApplicationPools.FirstOrDefault(x => x.Name == param.AppPoolName);

                    if (appPool != null
                        && appPool.State == ObjectState.Stopped)
                    {
                        appPool.Start();
                        Console.WriteLine("App Pool started.");
                    }
                }
#endif
                
                Console.WriteLine("Database Creation/Migration for version {0} finished", CurrentVersion);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "-" + e);
                logger.Fatal(e.ToString());
                logger.Fatal(e.Message, e);
                return 1;
            }
            return 0;
            // ReSharper restore LocalizableElement
        }

        public static bool IsUpdate { get; set; }

        public static void UpdateSchema(DatabaseInitializerParams param)
        {
            Console.WriteLine("Update Schemas");

            StopAppPools(param);

            DbMigrationsConfiguration configuration = new apcurium.MK.Booking.Migrations.ConfigMigrationBookingContext();
            configuration.TargetDatabase = new DbConnectionInfo(param.MkWebConnectionString, "System.Data.SqlClient");

            var migrator = new DbMigrator(configuration);
            DisplayPendingMigrations(migrator);
            migrator.Update();
            

            configuration = new apcurium.MK.Common.Migrations.ConfigMigrationConfigurationContext
            {
                TargetDatabase = new DbConnectionInfo(param.MkWebConnectionString, "System.Data.SqlClient")
            };

            migrator = new DbMigrator(configuration);
            DisplayPendingMigrations(migrator);
            migrator.Update();
            Console.WriteLine("Update Schemas Done");
        }

        private static void DisplayPendingMigrations(DbMigrator migrator)
        {
            var pendingMigrations = migrator.GetPendingMigrations().ToList();
            if (pendingMigrations.Any())
            {
                Console.WriteLine("Migration(s) To Be Applied on {0} context", migrator.Configuration.GetType().Name);
                foreach (var migration in pendingMigrations)
                {
                    Console.WriteLine(migration);
                }
            }
            else
            {
                Console.WriteLine("No Pending Migration To Be Applied for {0} context", migrator.Configuration.GetType().Name);
            }
        }

        private static void StopAppPools(DatabaseInitializerParams param)
        {
            Console.WriteLine("Stop App Pool to finish Database Migration...");
            var iisManager = new ServerManager();
            var appPool = iisManager.ApplicationPools.FirstOrDefault(x => x.Name == param.AppPoolName);

            if (appPool != null
                && appPool.State == ObjectState.Started)
            {
                appPool.Stop();
                Console.WriteLine("App Pool stopped.");
            }

            if (param.SecondWebServerName.HasValue())
            {
                try
                {
                    Console.WriteLine("Stop Secondary App Pool ...");
                    using (var remoteServerManager = ServerManager.OpenRemote(param.SecondWebServerName))
                    {
                        var remoteAppPool = remoteServerManager.ApplicationPools.FirstOrDefault(x => x.Name == param.AppPoolName);

                        if (remoteAppPool != null && remoteAppPool.State == ObjectState.Started)
                        {
                            remoteAppPool.Stop();
                            Console.WriteLine("Remote App Pool stopped.");
                        }
                        else if (remoteAppPool == null)
                        {
                            Console.WriteLine("No AppPool named {0} found at {1}", param.SecondWebServerName, param.AppPoolName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to connect to remote server {0}", param.SecondWebServerName);
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void SetupMirroring(DatabaseInitializerParams param)
        {
            if (!string.IsNullOrEmpty(param.MirroringSharedFolder))
            {
                Console.WriteLine("Add Mirroring on the new Database ...");
                var creatorDb = new DatabaseCreator();

                Console.WriteLine("MirroringSharedFolder :" + param.MirroringSharedFolder);
                var backupFolder = Path.Combine(param.MirroringSharedFolder,
                    param.CompanyName + DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss"));
                Directory.CreateDirectory(backupFolder);

                creatorDb.InitMirroring(param.MasterConnectionString, param.CompanyName);

                var mirrorDbExist = creatorDb.DatabaseExists(param.MirrorMasterConnectionString, param.CompanyName);
                if (mirrorDbExist)
                {
                    Console.WriteLine("Deleting existing mirroring db...");
                    creatorDb.DropDatabase(param.MirrorMasterConnectionString, param.CompanyName, false);
                }

                Console.WriteLine("Backup for mirroring...");
                creatorDb.BackupDatabase(param.MasterConnectionString, backupFolder, param.CompanyName);

                Console.WriteLine("Restoring mirroring backup...");
                creatorDb.RestoreDatabase(param.MirrorMasterConnectionString, backupFolder, param.CompanyName);

                Console.WriteLine("Set Mirroring Partner...");
                creatorDb.SetMirroringPartner(param.MirrorMasterConnectionString, param.CompanyName, param.MirroringMirrorPartner);
                Console.WriteLine("Complete Mirroring...");
                creatorDb.CompleteMirroring(param.MasterConnectionString, param.CompanyName, param.MirroringPrincipalPartner, param.MirroringWitness);
                Console.WriteLine("Mirroring Completed.");
            }
        }

        private static void CheckCompanyCreated(UnityContainer container, ICommandBus commandBus)
        {
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
        }

        private static void UpdateRatings(ICommandBus commandBus, IEnumerable<RatingTypeDetail[]> ratingTypes)
        {
            var supportedLanguages = Enum.GetNames(typeof(SupportedLanguages));

            foreach (var ratingType in ratingTypes)
            {
                var ratingTypeLanguages = ratingType.Select(t => t.Language);
                var missingRatingTypeLanguages = supportedLanguages.Except(ratingTypeLanguages).ToArray();

                if (missingRatingTypeLanguages.Any())
                {
                    // Every rating share the same ID for one language
                    var ratingId = ratingType.First().Id;

                    // Take english name by default. If none, take first that we find.
                    var ratingName =
                            ratingType.FirstOrDefault(r => r.Language == SupportedLanguages.en.ToString())
                                      .SelectOrDefault(r => r.Name);

                    if (ratingName.IsNullOrEmpty())
                    {
                        ratingName = ratingType.First().Name;
                    }

                    // Add missing language
                    foreach (var missingRatingTypeLanguage in missingRatingTypeLanguages)
                    {
                        commandBus.Send(new AddRatingType
                        {
                            CompanyId = AppConstants.CompanyId,
                            Name = ratingName,
                            RatingTypeId = ratingId,
                            Language = missingRatingTypeLanguage
                        });
                    }
                }
            }
        }

        private static void SetDefaultAdminSettings(IServerSettings serverSettings, IDictionary<string, string> appSettings)
        {
            if (!serverSettings.ServerData.SettingsAvailableToAdmin.HasValue())
            {
                // No settings are visible to admin, make default settings visible
                Console.WriteLine("Setting default admin settings...");

                var settingsAvailableToAdmin = new StringBuilder();
                var settings = serverSettings.ServerData.GetType().GetAllProperties();

                foreach (var setting in settings)
                {
                    var settingAttributes = setting.Value.GetCustomAttributes(false);
                    var isSettingAvailableToAdmin =
                        settingAttributes.OfType<CustomizableByCompanyAttribute>().FirstOrDefault();

                    if (isSettingAvailableToAdmin != null)
                    {
                        if (settingsAvailableToAdmin.Length > 0)
                        {
                            settingsAvailableToAdmin.Append(',');
                        }

                        settingsAvailableToAdmin.Append(setting.Key);
                    }
                }

                appSettings["SettingsAvailableToAdmin"] = settingsAvailableToAdmin.ToString();
            }
        }

        private static void AddDefaultRatings(ICommandBus commandBus)
        {
            var knowledgeId = Guid.NewGuid();
            var politenessId = Guid.NewGuid();
            var safetyId = Guid.NewGuid();
            var supportedLanguages = Enum.GetNames(typeof(SupportedLanguages));

            foreach (var supportedLanguage in supportedLanguages)
            {
                commandBus.Send(new AddRatingType
                {
                    CompanyId = AppConstants.CompanyId,
                    Name = "Knowledgable driver",
                    RatingTypeId = knowledgeId,
                    Language = supportedLanguage
                });
            }

            foreach (var supportedLanguage in supportedLanguages)
            {
                commandBus.Send(new AddRatingType
                {
                    CompanyId = AppConstants.CompanyId,
                    Name = "Politeness",
                    RatingTypeId = politenessId,
                    Language = supportedLanguage
                });
            }

            foreach (var supportedLanguage in supportedLanguages)
            {
                commandBus.Send(new AddRatingType
                {
                    CompanyId = AppConstants.CompanyId,
                    Name = "Safety",
                    RatingTypeId = safetyId,
                    Language = supportedLanguage
                });
            }
        }

	    private static void CreateAppleTestAccountIfNeeded(UnityContainer container, ICommandBus commandBus)
	    {
		    var accountDao = container.Resolve<IAccountDao>();

		    if (accountDao.FindByEmail("appletest@taxihail.com") != null)
		    {
				//Account is already present.
			    return;
		    }

			Console.WriteLine(@"Registering test account for Apple");
			//Register normal account
			var registerAccountCommand = new RegisterAccount
			{
				Id = Guid.NewGuid(),
				AccountId = Guid.NewGuid(),
				Email = "appletest@taxihail.com",
				Name = "John Doe",
				Country = new CountryISOCode("CA"),
				Phone = "6132875020",
				Password = "W$yQv9R"
			};

			var confirmationToken = Guid.NewGuid();
			registerAccountCommand.ConfimationToken = confirmationToken.ToString();

			commandBus.Send(registerAccountCommand);

			commandBus.Send(new ConfirmAccount
			{
				AccountId = registerAccountCommand.AccountId,
				ConfimationToken = registerAccountCommand.ConfimationToken
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
                Country = new CountryISOCode("CA"),
                Phone = "6132875020",
                Password = "password"
            };

            var confirmationToken = Guid.NewGuid();
            registerAccountCommand.ConfimationToken = confirmationToken.ToString();

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
                Country = new CountryISOCode("CA"),
                Phone = "6132875020",
                Password = "1l1k3B4n4n@", //Todo Make the admin portal customizable
                IsAdmin = true
            };

            var confirmationAdminToken = Guid.NewGuid();
            registerAdminAccountCommand.ConfimationToken = confirmationAdminToken.ToString();

            commandBus.Send(registerAdminAccountCommand);
            commandBus.Send(new UpdateRoleToUserAccount
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
                var paramFile = Path.Combine(AssemblyDirectory, args[0].Trim().Substring(2, args[0].Trim().Length - 2));
                Console.WriteLine("Reading param file : " + paramFile);
                if (!File.Exists(paramFile))
                {
                    throw new ApplicationException("Parameteres file cannot be found");
                }
                var paramFileContent = File.ReadAllText(paramFile);

                result = JsonSerializer.DeserializeFromString<DatabaseInitializerParams>(paramFileContent);
            }
            result.CompanyName = string.IsNullOrWhiteSpace(result.CompanyName) ? LocalDevProjectName : result.CompanyName;

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
                Console.WriteLine("Sql Directory Default is " + result.SqlServerDirectory);
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

            if (string.IsNullOrWhiteSpace(result.AppPoolName))
            {
                result.AppPoolName = "DefaultAppPool";
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
            var ibsServiceProvider = container.Resolve<IIBSServiceProvider>();

            var defaultCompany = ibsServiceProvider.StaticData().GetCompaniesList()
                .FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value)
                                 ?? ibsServiceProvider.StaticData().GetCompaniesList().FirstOrDefault();

            if (defaultCompany != null)
            {
                appSettings["DefaultBookingSettings.ProviderId"] = defaultCompany.Id.ToString();

                var defaultvehicule =
                    ibsServiceProvider.StaticData().GetVehiclesList(defaultCompany)
                        .FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value) ??
                    ibsServiceProvider.StaticData().GetVehiclesList(defaultCompany).First();
                appSettings["DefaultBookingSettings.VehicleTypeId"] = defaultvehicule.Id.ToString();

                appSettings["DefaultBookingSettings.ChargeTypeId"] = ChargeTypes.PaymentInCar.Id.ToString();
            }

            //Save settings so that registerAccountCommand succeed
            AddOrUpdateAppSettings(commandBus, appSettings);
        }

        private static void EnsureSupportRoleIsSupported(ConnectionStringSettings connectionString, ICommandBus commandBus)
        {
            var accounts = new AccountDao(() => new BookingDbContext(connectionString.ConnectionString));

            var allAdmins = accounts.GetAll();
            var admins = allAdmins.Where(a => a.Roles == (int)OldRoles.Admin);
            foreach (var admin in admins)
            {
                commandBus.Send(new UpdateRoleToUserAccount
                {
                    AccountId = admin.Id,
                    RoleName = RoleName.Admin,
                });
            }

            var superAdmins = allAdmins.Where(a => a.Roles == (int)OldRoles.SuperAdmin);
            foreach (var admin in superAdmins)
            {
                commandBus.Send(new UpdateRoleToUserAccount
                {
                    AccountId = admin.Id,
                    RoleName = RoleName.SuperAdmin,
                });
            }

        }

        private static void EnsureDefaultAccountsHasCorrectSettings(ConnectionStringSettings connectionString, ICommandBus commandBus)
        {
            var accounts = new AccountDao(() => new BookingDbContext(connectionString.ConnectionString));
            var admin = accounts.FindByEmail("taxihail@apcurium.com");

            if (admin != null
                && (!admin.HasAdminAccess || !admin.IsConfirmed))
            {
                commandBus.Send(new UpdateRoleToUserAccount
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
                    Country = new CountryISOCode("CA"),
                    Phone = "6132875020",
                    Name = "John Doe",
                    Email = "john@taxihail.com",
                    NumberOfTaxi = john.Settings.NumberOfTaxi,
                    ChargeTypeId = john.Settings.ChargeTypeId,
                    DefaultTipPercent = john.DefaultTipPercent
                };
                commandBus.Send(updateAccountCommand);
            }
        }

        private static void CheckandMigrateDefaultRules(ConnectionStringSettings connectionString, ICommandBus commandBus, IDictionary<string, string> appSettings)
        {
            var rules = new RuleDao(() => new BookingDbContext(connectionString.ConnectionString));
            if (
                rules.GetAll()
                    .None(r => (r.Category == (int)RuleCategory.WarningRule) && (r.Type == (int)RuleType.Default)))
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
                    .None(r => (r.Category == (int)RuleCategory.DisableRule) && (r.Type == (int)RuleType.Default)))
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

            var priority = rules.GetAll().Max(x => x.Priority) + 1;

            //validation of the pickup zone
            if (rules.GetAll()
                    .None(r => (r.Category == (int)RuleCategory.DisableRule)
                                && r.Type == (int)RuleType.Default
                                && r.AppliesToPickup
                                && r.ZoneRequired))
            {
                //Validation of the pickup
                commandBus.Send(new CreateRule
                {
                    Type = RuleType.Default,
                    Category = RuleCategory.DisableRule,
                    CompanyId = AppConstants.CompanyId,
                    AppliesToCurrentBooking = true,
                    AppliesToFutureBooking = true,
                    AppliesToPickup = true,
                    ZoneRequired = true,
                    RuleId = Guid.NewGuid(),
                    Name = "Pickup Zone Required",
                    Message = "The specified Pickup address lies outside the regular service area.",
                    IsActive = true,
                    Priority = priority++
                });
            }

            //check if any pickup exclusion
            if (appSettings.ContainsKey("IBS.PickupZoneToExclude")
                && !string.IsNullOrWhiteSpace(appSettings["IBS.PickupZoneToExclude"])
                && rules.GetAll()
                    .None(r => (r.Category == (int)RuleCategory.DisableRule)
                                && r.Type == (int)RuleType.Default
                                && r.AppliesToPickup
                                && r.ZoneList == appSettings["IBS.PickupZoneToExclude"]))
            {

                commandBus.Send(new CreateRule
                {
                    Type = RuleType.Default,
                    Category = RuleCategory.DisableRule,
                    CompanyId = AppConstants.CompanyId,
                    AppliesToCurrentBooking = true,
                    AppliesToFutureBooking = true,
                    AppliesToPickup = true,
                    ZoneList = appSettings["IBS.PickupZoneToExclude"],
                    RuleId = Guid.NewGuid(),
                    Name = "Pickup Zone Exclusion",
                    Message = "We don't serve this pickup location",
                    IsActive = true,
                    Priority = priority++
                });

                appSettings["IBS.PickupZoneToExclude"] = null;
            }

            //check if dropoff zone is required
            if (appSettings.ContainsKey("IBS.ValidateDestinationZone")
                && !string.IsNullOrWhiteSpace(appSettings["IBS.ValidateDestinationZone"])
                && rules.GetAll()
                    .None(r => (r.Category == (int)RuleCategory.DisableRule)
                                && r.Type == (int)RuleType.Default
                                && r.AppliesToDropoff
                                && r.ZoneRequired))
            {

                commandBus.Send(new CreateRule
                {
                    Type = RuleType.Default,
                    Category = RuleCategory.DisableRule,
                    CompanyId = AppConstants.CompanyId,
                    AppliesToCurrentBooking = true,
                    AppliesToFutureBooking = true,
                    AppliesToDropoff = true,
                    ZoneRequired = true,
                    RuleId = Guid.NewGuid(),
                    Name = "Dropoff Zone Required",
                    Message = "The specified Dropoff address lies outside the regular service area",
                    IsActive = bool.Parse(appSettings["IBS.ValidateDestinationZone"]),
                    Priority = priority++
                });
                appSettings["IBS.ValidateDestinationZone"] = null;
            }

            //check if any dropoff exclusion
            if (appSettings.ContainsKey("IBS.DestinationZoneToExclude")
                && !string.IsNullOrWhiteSpace(appSettings["IBS.DestinationZoneToExclude"])
                && rules.GetAll()
                    .None(r => (r.Category == (int)RuleCategory.DisableRule)
                                && r.Type == (int)RuleType.Default
                                && r.AppliesToDropoff
                                && r.ZoneList == appSettings["IBS.DestinationZoneToExclude"]))
            {

                commandBus.Send(new CreateRule
                {
                    Type = RuleType.Default,
                    Category = RuleCategory.DisableRule,
                    CompanyId = AppConstants.CompanyId,
                    AppliesToCurrentBooking = true,
                    AppliesToFutureBooking = true,
                    AppliesToDropoff = true,
                    ZoneList = appSettings["IBS.DestinationZoneToExclude"],
                    RuleId = Guid.NewGuid(),
                    Name = "Dropoff Zone Exclusion",
                    Message = "We don't serve this dropoff location",
                    IsActive = true,
                    Priority = priority++
                });
                appSettings["IBS.DestinationZoneToExclude"] = null;
            }

            AddOrUpdateAppSettings(commandBus, appSettings);
        }

        private static Dictionary<string, string> GetCompanySettings(string companyName)
        {
            // Create settings
            var appSettings = new Dictionary<string, string>();

            Console.WriteLine("Loading company settings...");

            var jsonSettings = File.ReadAllText(Path.Combine(AssemblyDirectory, "Settings\\", companyName + ".json"));
            var objectSettings = JObject.Parse(jsonSettings);

            foreach (var token in objectSettings)
            {
                appSettings[token.Key] = token.Value.ToString();
            }

            return appSettings;
        }

        private static void AddOrUpdateAppSettings(ICommandBus commandBus, IDictionary<string, string> appSettings)
        {
            commandBus.Send(new AddOrUpdateAppSettings
            {
                AppSettings = appSettings,
                CompanyId = AppConstants.CompanyId
            });
        }

        private static void CreateDefaultTariff(string connectionString, IServerSettings serverSettings, ICommandBus commandBus)
        {
            var tariffs = new TariffDao(() => new BookingDbContext(connectionString));
            if (tariffs.GetAll().All(x => x.Type != (int)TariffType.Default))
            {
                commandBus.Send(new CreateTariff
                {
                    Type = TariffType.Default,
                    KilometricRate = serverSettings.ServerData.Direction.RatePerKm,
                    FlatRate = serverSettings.ServerData.Direction.FlateRate,
                    MarginOfError = 20,
                    CompanyId = AppConstants.CompanyId,
                    TariffId = Guid.NewGuid(),
                });
            }
        }

        private static void MigratePaymentSettings(IServerSettings serverSettings, ICommandBus commandBus)
        {
            var paymentSettings = serverSettings.GetPaymentSettings();
            var needsUpdate = false;

            if (paymentSettings.AutomaticPaymentPairing)
            {
                paymentSettings.IsUnpairingDisabled = true;
                paymentSettings.AutomaticPaymentPairing = false;
                needsUpdate = true;
            }

            if (paymentSettings.NoShowFee.HasValue)
            {
                var noShowFee = paymentSettings.NoShowFee.Value;
                commandBus.Send(new UpdateFees
                {
                    CompanyId = AppConstants.CompanyId,
                    Fees = new List<Fees>
                    {
                        new Fees
                        {
                            NoShow = noShowFee
                        }
                    }
                });

                paymentSettings.NoShowFee = null;
                needsUpdate = true;
            }

            if (serverSettings.ServerData.UsePairingCodeWhenUsingRideLinqCmtPayment)
            {
                paymentSettings.CmtPaymentSettings.UsePairingCode = true;
                needsUpdate = true;
            }

            if (serverSettings.ServerData.CreditCardIsMandatory)
            {
                paymentSettings.CreditCardIsMandatory = true;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                commandBus.Send(new UpdatePaymentSettings
                {
                    CompanyId = AppConstants.CompanyId,
                    ServerPaymentSettings = paymentSettings
                });
            }
        }

        private static void CreateDefaultVehicleTypes(UnityContainer container, ICommandBus commandBus)
        {
            var referenceDataService = container.Resolve<ReferenceDataService>();
            var referenceData = (ReferenceData)referenceDataService.Get(new ReferenceDataRequest());

            foreach (var company in referenceData.CompaniesList)
            {
                var vehicles = container.Resolve<IIBSServiceProvider>().StaticData().GetVehicles(company);
                foreach (var vehicle in vehicles)
                {
                    commandBus.Send(new AddUpdateVehicleType
                    {
                        VehicleTypeId = Guid.NewGuid(),
                        Name = string.Format("{0}", vehicle.Name),
                        LogoName = "taxi",
                        ReferenceDataVehicleId = vehicle.ID,
                        CompanyId = AppConstants.CompanyId,
                        MaxNumberPassengers = vehicle.Capacity
                    });
                }

            }
        }

        private static void CheckAndAddCapacity(IEnumerable<VehicleTypeDetail> vehicleTypeDetails, UnityContainer container, ICommandBus commandBus)
        {
            var currentVersion = Assembly.GetAssembly(typeof(Program)).GetName().Version;
            var versionIntroduceCapacity = new Version(2, 2, 1);

            //before version 2.2.1 vehicle type have no capacity, we need to set the value from IBS if any
            if (currentVersion.CompareTo(versionIntroduceCapacity) <= 0)
            {
                var referenceDataService = container.Resolve<ReferenceDataService>();
                var referenceData = (ReferenceData)referenceDataService.Get(new ReferenceDataRequest());
                var ibsVehicleData = new List<TVehicleTypeItem>();

                foreach (var company in referenceData.CompaniesList)
                {
                    ibsVehicleData.AddRange(container.Resolve<IIBSServiceProvider>().StaticData().GetVehicles(company));
                }

                foreach (var typeDetail in vehicleTypeDetails)
                {
                    var ibsData = ibsVehicleData.FirstOrDefault(x => x.ID == typeDetail.ReferenceDataVehicleId);
                    if (ibsData != null)
                    {
                        commandBus.Send(new AddUpdateVehicleType
                        {
                            VehicleTypeId = typeDetail.Id,
                            Name = string.Format("{0}", typeDetail.Name),
                            LogoName = typeDetail.LogoName,
                            ReferenceDataVehicleId = typeDetail.ReferenceDataVehicleId,
                            CompanyId = AppConstants.CompanyId,
                            MaxNumberPassengers = ibsData.Capacity
                        });
                    }
                }
            }
        }

        private static void EnsurePrivacyPolicyExists(ConnectionStringSettings connectionString, ICommandBus commandBus, IServerSettings serverSettings)
        {
            var company = new CompanyDao(() => new BookingDbContext(connectionString.ConnectionString));
            if (!company.Get().PrivacyPolicy.HasValue())
            {
                var privacyTemplate = File.ReadAllText(@"DefaultPrivacy.txt");

                commandBus.Send(new UpdatePrivacyPolicy
                {
                    CompanyId = AppConstants.CompanyId,
                    Policy = string.Format(privacyTemplate, serverSettings.ServerData.TaxiHail.ApplicationName, serverSettings.ServerData.SupportEmail)
                });
            }
        }
    }
}
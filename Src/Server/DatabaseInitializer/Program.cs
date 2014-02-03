#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
using apcurium.MK.Common.Extensions;
using DatabaseInitializer.Services;
using DatabaseInitializer.Sql;
using Infrastructure.Messaging;
using log4net;
using Microsoft.Practices.Unity;
using Newtonsoft.Json.Linq;
using ConfigurationManager = apcurium.MK.Common.Configuration.Impl.ConfigurationManager;

#endregion

namespace DatabaseInitializer
{
    public class Program
    {
        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static int Main(string[] args)
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
                    string.Format(
                        "Data Source=.;Initial Catalog={0};Integrated Security=True; MultipleActiveResultSets=True",
                        companyName));
                var connStringMaster = connectionString.ConnectionString.Replace(companyName, "master");

                //SQL Instance name
                var sqlInstanceName = "MSSQL11.MSSQLSERVER";
                if (args.Length > 1)
                {
                    sqlInstanceName = args[1];
                }
                else
                {
// ReSharper disable LocalizableElement
                    Console.WriteLine("Sql Instance name ? Default is MSSQL11.MSSQLSERVER , 1- MSSQL10_50.MSSQLSERVER");

                    var userSqlInstance = Console.ReadLine();

                    sqlInstanceName = string.IsNullOrEmpty(userSqlInstance)
                        ? sqlInstanceName
                        : userSqlInstance == "1" ? "MSSQL10_50.MSSQLSERVER" : userSqlInstance;
                }


                Console.WriteLine("Working...");

                var creatorDb = new DatabaseCreator();
                string oldDatabase = null;
                var container = new UnityContainer();
                var module = new Module();
                module.Init(container, connectionString);

                var configurationManager = new
                    ConfigurationManager(
                    () => new ConfigurationDbContext(connectionString.ConnectionString), container.Resolve<ILogger>());

                IDictionary<string, string> settingsInDb = null;

                var isUpdate = creatorDb.DatabaseExists(connStringMaster, companyName);

                //for dev company, delete old database to prevent keeping too many databases
                if (companyName == "MKWebDev"
                    && isUpdate)
                {
#if DEBUG
                    Console.WriteLine("Drop Existing Database? Y or N");
                    var shouldDrop = args.Length > 2 ? args[2] : Console.ReadLine();
                    if ("Y".Equals(shouldDrop, StringComparison.OrdinalIgnoreCase))
                    {
                        creatorDb.DropDatabase(connStringMaster, companyName);
                        isUpdate = false;
                    }
#endif
                }

                if (isUpdate)
                {
                    settingsInDb = configurationManager.GetSettings();
                    oldDatabase = creatorDb.RenameDatabase(connStringMaster, companyName);
                }

                creatorDb.CreateDatabase(connStringMaster, companyName, sqlInstanceName);
                creatorDb.CreateSchemas(connectionString);

                Console.WriteLine("Add user for IIS...");

                ////add user for IIS IIS APPPOOL\MyCompany
                if (companyName != "MKWebDev")
                {
                    creatorDb.AddUserAndRighst(connStringMaster, connectionString.ConnectionString,
                        "IIS APPPOOL\\" + companyName, companyName);
                }

                //Copy Domain Events
                if (isUpdate)
                {
                    creatorDb.CopyEventsAndCacheTables(connStringMaster, oldDatabase, companyName);
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
                //Create settings

                var appSettings = new Dictionary<string, string>();
                var jsonSettings = File.ReadAllText(Path.Combine(AssemblyDirectory, "Settings\\Common.json"));
                var objectSettings = JObject.Parse(jsonSettings);

                Console.WriteLine("Loading settings...");
                if (isUpdate)
                {
                    JObject settings = objectSettings;
                    settingsInDb.ForEach(setting => { settings[setting.Key] = setting.Value; });
                }
                foreach (var token in objectSettings)
                {
                    appSettings[token.Key] = token.Value.ToString();
                }

                jsonSettings = File.ReadAllText(Path.Combine(AssemblyDirectory, "Settings\\", companyName + ".json"));
                objectSettings = JObject.Parse(jsonSettings);
                if (isUpdate)
                {
                    settingsInDb.ForEach(setting => objectSettings[setting.Key] = setting.Value);
                }
                foreach (var token in objectSettings)
                {
                    appSettings[token.Key] = token.Value.ToString();
                }

                //Save settings so that next calls to referenceDataService has the IBS Url
                AddOrUpdateAppSettings(commandBus, appSettings);


                if (isUpdate)
                {
                    Console.WriteLine("Playing events...");
                    //migrate events
                    var migrators = container.ResolveAll<IEventsMigrator>();
                    foreach (var eventsMigrator in migrators)
                    {
                        eventsMigrator.Do(appSettings["TaxiHail.Version"]);
                    }

                    //replay events
                    var replayService = container.Resolve<IEventsPlayBackService>();
                    replayService.ReplayAllEvents();

                    var tariffs = new TariffDao(() => new BookingDbContext(connectionString.ConnectionString));
                    if (tariffs.GetAll().All(x => x.Type != (int) TariffType.Default))
                    {
                        // Default rate does not exist for this company 
                        CreateDefaultTariff(configurationManager, commandBus);
                    }

                    CreateDefaultRules(connectionString, commandBus);
                    Console.WriteLine("Done playing events...");

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
                else
                {
                    appSettings.Clear();
                    // Create default rate for company
                    CreateDefaultTariff(configurationManager, commandBus);
                    CreateDefaultRules(connectionString, commandBus);

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

                        var defaultchargetype = referenceDataService.GetPaymentsList(defaultCompany)
                            .FirstOrDefault(x => x.Display.HasValue() && x.Display.Contains("Cash"))
                                                ?? referenceDataService.GetPaymentsList(defaultCompany).First();


                        appSettings["DefaultBookingSettings.ChargeTypeId"] = defaultchargetype.Id.ToString();
                    }

                    //Save settings so that registerAccountCommand succeed
                    AddOrUpdateAppSettings(commandBus, appSettings);
                    appSettings.Clear();

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

                commandBus.Send(new AddOrUpdateAppSettings
                {
                    AppSettings = appSettings,
                    CompanyId = AppConstants.CompanyId
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
                loggger.Fatal(e.Message, e);
                return 1;
            }
            return 0;
// ReSharper restore LocalizableElement
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
                PassengerRate = 0m,
                CompanyId = AppConstants.CompanyId,
                TariffId = Guid.NewGuid(),
            });
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using DatabaseInitializer.Services;
using DatabaseInitializer.Sql;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using Newtonsoft.Json.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
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

                var connectionString = new ConnectionStringSettings("MkWeb", string.Format( "Data Source=.;Initial Catalog={0};Integrated Security=True; MultipleActiveResultSets=True", companyName ));
                var connStringMaster = connectionString.ConnectionString.Replace(companyName, "master");

                //Init or Update
                bool isUpdate;
                if (args.Length > 1)
                {
                    isUpdate = args[1].ToUpperInvariant() == "U";
                }
                else
                {
                    Console.WriteLine("[C]reate (drop existing) or [U]pdate database ?");
                    isUpdate = Console.ReadLine().ToUpperInvariant() == "U";
                }

                //SQL Instance name
                var sqlInstanceName = "MSSQL11.MSSQLSERVER";
                if (args.Length > 2)
                {
                    sqlInstanceName = args[2];
                }
                else
                {
                    Console.WriteLine("Sql Instance name ? Default is MSSQL11.MSSQLSERVER , 1- MSSQL10_50.MSSQLSERVER");
                    var userSqlInstance = Console.ReadLine();

                    sqlInstanceName = string.IsNullOrEmpty(userSqlInstance) ? sqlInstanceName : userSqlInstance == "1" ?"MSSQL10_50.MSSQLSERVER" : userSqlInstance;
                }

                var creatorDb = new DatabaseCreator();
                string oldDatabase = null;
                IConfigurationManager configurationManager = new
                    apcurium.MK.Common.Configuration.Impl.ConfigurationManager(
                    () => new ConfigurationDbContext(connectionString.ConnectionString));
                IDictionary<string, string> settingsInDb = null;
                if (isUpdate)
                {
                   settingsInDb = configurationManager.GetSettings();
                   oldDatabase = creatorDb.RenameDatabase(connStringMaster, companyName);
                }

                creatorDb.CreateDatabase(connStringMaster, companyName, sqlInstanceName);
                creatorDb.CreateSchemas(connectionString);

                ////add user for IIS IIS APPPOOL\MyCompany
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
                //Init container
                var container = new UnityContainer();
                var module = new Module();
                module.Init(container, connectionString);

                //Init data
                var commandBus = container.Resolve<ICommandBus>();

                   bool companyIsCreated = container.Resolve<IEventsPlayBackService>().CountEvent("Company") >0;

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
                if(isUpdate)
                {
                    settingsInDb.ForEach(setting =>
                                             {
                                                 objectSettings[setting.Key] = setting.Value;
                                             });
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
                appSettings = AddOrUpdateAppSettings(commandBus, appSettings);

                if (isUpdate)
                {
                    //migrate events
                    var migrator = container.Resolve<IEventsMigrator>();
                    migrator.Do();

                    //replay events
                    var replayService = container.Resolve<IEventsPlayBackService>();
                    replayService.ReplayAllEvents();

                    var tariffs = new TariffDao(() => new BookingDbContext(connectionString.ConnectionString));
                    if(tariffs.GetAll().All(x => x.Type != (int)TariffType.Default))
                    {
                       // Default rate does not exist for this company 
                        CreateDefaultTariff(configurationManager, commandBus);
                    }
                }
                else
                {

                    // Create default rate for company
                    CreateDefaultTariff(configurationManager, commandBus);


                    //Get default settings from IBS
                    var referenceDataService = container.Resolve<IStaticDataWebServiceClient>();
                    var defaultCompany = referenceDataService.GetCompaniesList().FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value) 
                                        ?? referenceDataService.GetCompaniesList().FirstOrDefault();
                    
                    if(defaultCompany != null)
                    {
                       appSettings["DefaultBookingSettings.ProviderId"] = defaultCompany.Id.ToString();

                        var defaultvehicule = referenceDataService.GetVehiclesList(defaultCompany).FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value) ??
                                              referenceDataService.GetVehiclesList(defaultCompany).First();
                       appSettings["DefaultBookingSettings.VehicleTypeId"] = defaultvehicule.Id.ToString();
                        
                        var defaultchargetype = referenceDataService.GetPaymentsList(defaultCompany).FirstOrDefault(x => x.Display.HasValue() && x.Display.Contains("Cash"))
                            ?? referenceDataService.GetPaymentsList(defaultCompany).First();

                        
                        appSettings["DefaultBookingSettings.ChargeTypeId"] = defaultchargetype.Id.ToString();
                        
                    }

                    //Save settings so that registerAccountCommand succeed
                    appSettings = AddOrUpdateAppSettings(commandBus, appSettings);

                    //Register normal account
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




                    //Register admin account

                    var registerAdminAccountCommand = new RegisterAccount
                    {
                        Id = Guid.NewGuid(),
                        AccountId = Guid.NewGuid(),
                        Email = "taxihail@apcurium.com",
                        Name = "Administrator",
                        Phone = "5146543024",
                        Password = "1l1k3B4n4n@",
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


                    commandBus.Send(new ConfirmAccount
                    {
                        AccountId = registerAdminAccountCommand.AccountId,
                        ConfimationToken = registerAdminAccountCommand.ConfimationToken
                    });

                    //TODO: Add default rating types
                    //commandBus.Send(new AddRatingType
                    //{
                    //    CompanyId = AppConstants.CompanyId, //TODO: Validate terms : Cab Cleanliness, Driver Politeness, Safety/Driving
                    //    Name = "Cab Cleanliness"
                    //});
                }

                commandBus.Send(new AddOrUpdateAppSettings()
                {
                    AppSettings = appSettings,
                    CompanyId = AppConstants.CompanyId
                });
                commandBus.Send(new AddRatingType()
                {
                    CompanyId = AppConstants.CompanyId,
                    Name = "Knowledgable driver",
                    RatingTypeId = Guid.NewGuid()
                });
                commandBus.Send(new AddRatingType()
                {
                    CompanyId = AppConstants.CompanyId,
                    Name = "Politness",
                    RatingTypeId = Guid.NewGuid()
                });
                commandBus.Send(new AddRatingType()
                {
                    CompanyId = AppConstants.CompanyId,
                    Name = "Safety",
                    RatingTypeId = Guid.NewGuid()
                });
            }catch(Exception e)
            {
                Console.WriteLine(e.Message + " " +e.StackTrace);
                loggger.Fatal(e.Message, e);
                return 1;
            }
            return 0;
        }

        private static Dictionary<string, string> AddOrUpdateAppSettings(ICommandBus commandBus, Dictionary<string, string> appSettings)
       { 
            commandBus.Send(new AddOrUpdateAppSettings
                                {
                                    AppSettings = appSettings,
                                    CompanyId = AppConstants.CompanyId
                                });

            return   new Dictionary<string, string>();
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

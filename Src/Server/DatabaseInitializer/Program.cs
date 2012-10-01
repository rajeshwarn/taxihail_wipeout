﻿using System;
using System.IO;
using System.Reflection;
using DatabaseInitializer.Services;
using DatabaseInitializer.Sql;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common.Configuration.Impl;

namespace DatabaseInitializer
{
    using System.Configuration;

    public class Program
    {
        public static void Main(string[] args)
        {
            var companyName = "MKWebDev";
            if (args.Length > 0)
            {
                companyName = args[0];
            }

            var connectionString = ConfigurationManager.ConnectionStrings[companyName];
            var connStringMaster = connectionString.ConnectionString.Replace(companyName, "master");

            //Init or Update
            var isUpdate = true;
            if (args.Length > 1)
            {
                isUpdate = args[1].ToUpperInvariant() == "U";
            }else
            {
                Console.WriteLine("[C]reate (drop existing) or [U]pdate database ? C/U");
                isUpdate = Console.ReadLine() == "U";
            }

            //SQL Instance name
            var sqlInstanceName = "MSSQL11.MSSQLSERVER";
            if (args.Length > 2)
            {
                sqlInstanceName = args[2];
            }else
            {
                Console.WriteLine("Sql Instance name ? Default is MSSQL11.MSSQLSERVER");
                var userSqlInstance = Console.ReadLine();
                sqlInstanceName = string.IsNullOrEmpty(userSqlInstance) ? sqlInstanceName : userSqlInstance;
            }

            var creatorDb = new DatabaseCreator();
            string oldDatabase = null;
            if (isUpdate)
            {
                oldDatabase = creatorDb.RenameDatabase(connStringMaster, companyName);
            }

            creatorDb.CreateDatabase(connStringMaster, companyName, sqlInstanceName);
            creatorDb.CreateSchemas(connectionString);

            //Copy Domain Events
            if (isUpdate)
            {
                creatorDb.CopyDomainEventFromOldToNewDatabase(connStringMaster, oldDatabase, companyName);
            }
            
            //Create settings
            var configurationManager = new
                apcurium.MK.Common.Configuration.Impl.ConfigurationManager(() => new ConfigurationDbContext(connectionString.ConnectionString));

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

            if(isUpdate)
            {
               //replay events
                var replayService = container.Resolve<IEventsPlayBackService>();
                replayService.ReplayAllEvents();

            }else
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
                registerAccountCommand.IbsAccountId = accountWebServiceClient.CreateAccount(registerAccountCommand.AccountId,
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

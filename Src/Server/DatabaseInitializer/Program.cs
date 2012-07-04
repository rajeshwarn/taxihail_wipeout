// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// ©2012 Microsoft. All rights reserved. Certain content used with permission from contributors
// http://cqrsjourney.github.com/contributors/members
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

using apcurium.MK.Common.Configuration.Impl;

namespace DatabaseInitializer
{
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    using Infrastructure.Sql.BlobStorage;
    using Infrastructure.Sql.MessageLog;
    using Infrastructure.Sql.EventSourcing;
    using Infrastructure.Sql.Messaging.Implementation;   
    using apcurium.MK.Booking.Database;

    public class Program
    {
        public static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.AppSettings["defaultConnection"];
            if (args.Length > 0)
            {
                connectionString = args[0];
            }

            // Use ConferenceContext as entry point for dropping and recreating DB
            using (var context = new BookingDbContext(connectionString))
            {
                context.Database.CreateIfNotExists();
            }

            using (var context = new ConfigurationDbContext(connectionString))
            {
                context.Database.CreateIfNotExists();
            }

            Database.SetInitializer<EventStoreDbContext>(null);
            Database.SetInitializer<MessageLogDbContext>(null);
            Database.SetInitializer<BlobStorageDbContext>(null);
            Database.SetInitializer<BookingDbContext>(null);
            Database.SetInitializer<ConfigurationDbContext>(null);
            
            DbContext[] contexts =
                new DbContext[] 
                { 
                    new ConfigurationDbContext(connectionString),
                    new EventStoreDbContext(connectionString),
                    new MessageLogDbContext(connectionString),
                    new BlobStorageDbContext(connectionString) 
                };

            try
            {

                foreach (DbContext context in contexts)
                {
                    var adapter = (IObjectContextAdapter) context;

                    var script = adapter.ObjectContext.CreateDatabaseScript();

                    context.Database.ExecuteSqlCommand(script);

                    context.Dispose();
                }

            }catch
            {
                //TODO trouver un moyen plus sexy
            }

            MessagingDbInitializer.CreateDatabaseObjects(connectionString, "SqlBus");

            var configurationManager = new
                apcurium.MK.Common.Configuration.Impl.ConfigurationManager(() => new ConfigurationDbContext(connectionString));

            configurationManager.SetSetting("IBS.WebServicesUserName", "taxi");
            configurationManager.SetSetting("IBS.WebServicesPassword", "test");
            configurationManager.SetSetting("IBS.WebServicesUrl", "http://drivelinq.dyndns-ip.com:6928/XDS_IASPI.DLL/soap/");
            configurationManager.SetSetting("IBS.DefaultAccountPassword", "password");

        }
    }
}

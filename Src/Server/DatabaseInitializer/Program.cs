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

            Database.SetInitializer<EventStoreDbContext>(null);
            Database.SetInitializer<MessageLogDbContext>(null);
            Database.SetInitializer<BlobStorageDbContext>(null);
            Database.SetInitializer<BookingDbContext>(null);
            
            DbContext[] contexts =
                new DbContext[] 
                { 
                    new EventStoreDbContext(connectionString),
                    new MessageLogDbContext(connectionString),
                    new BlobStorageDbContext(connectionString),                    
                };

            foreach (DbContext context in contexts)
            {
                var adapter = (IObjectContextAdapter)context;

                var script = adapter.ObjectContext.CreateDatabaseScript();

                context.Database.ExecuteSqlCommand(script);

                context.Dispose();
            }

            //using (var context = new PaymentsDbContext(connectionString))
            //{
            //    PaymentsReadDbContextInitializer.CreateViews(context);
            //}

            MessagingDbInitializer.CreateDatabaseObjects(connectionString, "SqlBus");
        }
    }
}

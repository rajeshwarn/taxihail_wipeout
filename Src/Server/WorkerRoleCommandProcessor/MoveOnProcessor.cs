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

using apcurium.MK.Booking.BackOffice.CommandHandlers;

namespace WorkerRoleCommandProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading;
    using Infrastructure;
    using Infrastructure.BlobStorage;
    using Infrastructure.Messaging.Handling;
    using Infrastructure.Serialization;
    using Infrastructure.Sql.BlobStorage;
    using Infrastructure.Sql.EventSourcing;
    using Infrastructure.Sql.MessageLog;
    using Microsoft.Practices.Unity;        
    using apcurium.MK.Booking.Database;
    using apcurium.MK.Booking.CommandHandlers;

    public sealed partial class MoveOnProcessor : IDisposable
    {
        private IUnityContainer container;
        private CancellationTokenSource cancellationTokenSource;
        private List<IProcessor> processors;

        public MoveOnProcessor()
        {
            OnCreating();

            this.cancellationTokenSource = new CancellationTokenSource();
            this.container = CreateContainer();
            RegisterCommandHandlers(container);
            

            Database.SetInitializer<EventStoreDbContext>(null);
            Database.SetInitializer<MessageLogDbContext>(null);
            Database.SetInitializer<BlobStorageDbContext>(null);
            Database.SetInitializer<BookingDbContext>(null);

            this.processors = this.container.ResolveAll<IProcessor>().ToList();
        }

        public void Start()
        {
            this.processors.ForEach(p => p.Start());
        }

        public void Stop()
        {
            this.cancellationTokenSource.Cancel();

            this.processors.ForEach(p => p.Stop());
        }

        public void Dispose()
        {
            this.container.Dispose();
        }

        private UnityContainer CreateContainer()
        {
            var container = new UnityContainer();

            // infrastructure
            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());

            container.RegisterType<IBlobStorage, SqlBlobStorage>(new ContainerControlledLifetimeManager(), new InjectionConstructor("BlobStorage"));            
                        
            //container.RegisterType<DbContext, BackOfficeDbContext>("backoffice", new TransientLifetimeManager(), new InjectionConstructor("BackOffice"));

            container.RegisterType<BookingDbContext>(new TransientLifetimeManager(), new InjectionConstructor("MKWeb"));
                        
            // handlers
            container.RegisterType<ICommandHandler, AccountCommandHandler>("AccountCommandHandler");
            container.RegisterType<ICommandHandler, FavoriteAddressCommandHandler>("FavoriteAddressCommandHandler");            
            

            OnCreateContainer(container);

            return container;
        }

        private static void RegisterCommandHandlers(IUnityContainer unityContainer)
        {
            var commandHandlerRegistry = unityContainer.Resolve<ICommandHandlerRegistry>();

            foreach (var commandHandler in unityContainer.ResolveAll<ICommandHandler>())
            {
                commandHandlerRegistry.Register(commandHandler);
            }
        }

        partial void OnCreating();
        partial void OnCreateContainer(UnityContainer container);
    }
}

﻿// ==============================================================================================================
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

using apcurium.MK.Booking.BackOffice.EventHandlers;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;

namespace WorkerRoleCommandProcessor
{
    using System.Data.Entity;
    using Infrastructure;
    using Infrastructure.EventSourcing;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Handling;
    using Infrastructure.Serialization;
    using Infrastructure.Sql.EventSourcing;
    using Infrastructure.Sql.MessageLog;
    using Infrastructure.Sql.Messaging;
    using Infrastructure.Sql.Messaging.Handling;
    using Infrastructure.Sql.Messaging.Implementation;
    using Microsoft.Practices.Unity;
    using apcurium.MK.Booking.EventHandlers;

    /// <summary>
    /// Local-side of the processor, which is included for compilation conditionally 
    /// at the csproj level.
    /// </summary>
    /// <devdoc>
    /// NOTE: this file is only compiled on DebugLocal configurations. In non-DebugLocal 
    /// you will not see full syntax coloring, intellisense, etc.. But it is still 
    /// much more readable and usable than a grayed-out piece of code inside an #if
    /// </devdoc>
    partial class MKBookingProcessor
    {
        partial void OnCreateContainer(UnityContainer container)
        {
            var serializer = container.Resolve<ITextSerializer>();
            var metadata = container.Resolve<IMetadataProvider>();

#if STAGING
            string databaseName = "MkWebStaging";   
#else
            string databaseName = "MkWeb";
#endif

            var commandBus = new CommandBus(new MessageSender(Database.DefaultConnectionFactory, databaseName, "SqlBus.Commands"), serializer);
            var eventBus = new EventBus(new MessageSender(Database.DefaultConnectionFactory, databaseName, "SqlBus.Events"), serializer);

            var commandProcessor = new CommandProcessor(new MessageReceiver(Database.DefaultConnectionFactory, databaseName, "SqlBus.Commands"), serializer);
            var eventProcessor = new EventProcessor(new MessageReceiver(Database.DefaultConnectionFactory, databaseName, "SqlBus.Events"), serializer);
            
            

            container.RegisterInstance<IAccountWebServiceClient>(new AccountWebServiceClient(container.Resolve<IConfigurationManager>(), new Logger()));

            container.RegisterInstance<ICommandBus>(commandBus);
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<ICommandHandlerRegistry>(commandProcessor);
            container.RegisterInstance<IProcessor>("CommandProcessor", commandProcessor);
            container.RegisterInstance<IEventHandlerRegistry>(eventProcessor);
            container.RegisterInstance<IProcessor>("EventProcessor", eventProcessor);

            // Event log database and handler.
            container.RegisterType<SqlMessageLog>(new InjectionConstructor("MessageLog", serializer, metadata));
            container.RegisterType<IEventHandler, SqlMessageLogHandler>("SqlMessageLogHandler");
            container.RegisterType<ICommandHandler, SqlMessageLogHandler>("SqlMessageLogHandler");

            RegisterRepository(container);
            RegisterEventHandlers(container, eventProcessor);
        }

        private void RegisterEventHandlers(UnityContainer container, EventProcessor eventProcessor)
        {
            eventProcessor.Register(container.Resolve<AccountDetailsGenerator>());
            eventProcessor.Register(container.Resolve<AddressListGenerator>());
            eventProcessor.Register(container.Resolve<OrderGenerator>());
            eventProcessor.Register(container.Resolve<SqlMessageLogHandler>());
        }

        private void RegisterRepository(UnityContainer container)
        {
            // repository
            container.RegisterType<EventStoreDbContext>(new TransientLifetimeManager(), new InjectionConstructor("EventStore"));
            container.RegisterType(typeof(IEventSourcedRepository<>), typeof(SqlEventSourcedRepository<>), new ContainerControlledLifetimeManager());
        }

    }
}

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

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Serialization;
using log4net;
using log4net.Core;

namespace Infrastructure.Messaging.InMemory
{
    using System.Collections.Generic;
    using System.Linq;
    using Handling;

    /// <summary>
    /// Sample in-memory command bus that is asynchronous.
    /// </summary>
    public class AsynchronousMemoryCommandBus : ICommandBus, ICommandHandlerRegistry
    {
        private readonly ITextSerializer _serializer;
        private Dictionary<Type, ICommandHandler> handlers = new Dictionary<Type, ICommandHandler>();

        private static readonly ILog Log = LogManager.GetLogger(typeof(AsynchronousMemoryCommandBus));      

        
        public AsynchronousMemoryCommandBus(ITextSerializer serializer, params ICommandHandler[] handlers)
        {
            _serializer = serializer;
            foreach (var commandHandler in handlers)
            {
                Register(commandHandler);
            }
        }

        public void Register(ICommandHandler commandHandler)
        {
            var genericHandler = typeof(ICommandHandler<>);
            var supportedCommandTypes = commandHandler.GetType()
                .GetInterfaces()
                .Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == genericHandler)
                .Select(iface => iface.GetGenericArguments()[0])
                .ToList();

            if (handlers.Keys.Any(registeredType => supportedCommandTypes.Contains(registeredType)))
                throw new ArgumentException("The command handled by the received handler already has a registered handler.");

            // Register this handler for each of the handled types.
            foreach (var commandType in supportedCommandTypes)
            {
                this.handlers.Add(commandType, commandHandler);
            }
        }

        public void Send(Envelope<ICommand> command)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (command.Delay > TimeSpan.Zero)
                    {
                        Thread.Sleep(command.Delay);
                    }

                    var commandType = command.Body.GetType();
                    ICommandHandler handler = null;

                    if (this.handlers.TryGetValue(commandType, out handler))
                    {                     
                        ((dynamic)handler).Handle((dynamic)command.Body);
                    }

                    // There can be a generic logging/tracing/auditing handlers
                    if (this.handlers.TryGetValue(typeof(ICommand), out handler))
                    {                        
                        ((dynamic)handler).Handle((dynamic)command.Body);
                    }
                   
                }
                catch (Exception e)
                {
                    var payload = _serializer.Serialize(command);
                    string innerException = string.Empty;
                    if (e.InnerException != null)
                    {
                        innerException = e.InnerException.ToString();
                    }
                    Log.Error("Error in handling command " + command.Body.GetType() + Environment.NewLine + payload + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine + innerException, e);
                    
                }
            });
        }

        public void Send(IEnumerable<Envelope<ICommand>> commands)
        {
            foreach (var command in commands)
            {
                this.Send(command);
            }
        }
    }
}

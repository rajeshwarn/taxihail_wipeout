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

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Infrastructure.Messaging.InMemory
{
    using System.Collections.Generic;
    using System.Linq;
    using Handling;

    public class AsynchronousMemoryEventBus : IEventBus, IEventHandlerRegistry
    {
        private Dictionary<Type, List<Tuple<Type, Action<Envelope>>>> handlersByEventType;
        private Dictionary<Type, Action<IEvent, string, string, string>> dispatchersByEventType;
        private List<IEvent> events = new List<IEvent>();

        public AsynchronousMemoryEventBus(params IEventHandler[] handlers)
        {
            this.handlersByEventType = new Dictionary<Type, List<Tuple<Type, Action<Envelope>>>>();
            this.dispatchersByEventType = new Dictionary<Type, Action<IEvent, string, string, string>>();
            foreach (var eventHandler in handlers)
            {
                Register(eventHandler);
            }
        }

        public void Register(IEventHandler handler)
        {
            var handlerType = handler.GetType();

            foreach (var invocationTuple in this.BuildHandlerInvocations(handler))
            {
                var envelopeType = typeof(Envelope<>).MakeGenericType(invocationTuple.Item1);

                List<Tuple<Type, Action<Envelope>>> invocations;
                if (!this.handlersByEventType.TryGetValue(invocationTuple.Item1, out invocations))
                {
                    invocations = new List<Tuple<Type, Action<Envelope>>>();
                    this.handlersByEventType[invocationTuple.Item1] = invocations;
                }
                invocations.Add(new Tuple<Type, Action<Envelope>>(handlerType, invocationTuple.Item2));

                if (!this.dispatchersByEventType.ContainsKey(invocationTuple.Item1))
                {
                    this.dispatchersByEventType[invocationTuple.Item1] = this.BuildDispatchInvocation(invocationTuple.Item1);
                }
            }
        }

        private IEnumerable<Tuple<Type, Action<Envelope>>> BuildHandlerInvocations(IEventHandler handler)
        {
            var interfaces = handler.GetType().GetInterfaces();

            var eventHandlerInvocations =
                interfaces
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                    .Select(i => new { HandlerInterface = i, EventType = i.GetGenericArguments()[0] })
                    .Select(e => new Tuple<Type, Action<Envelope>>(e.EventType, this.BuildHandlerInvocation(handler, e.HandlerInterface, e.EventType)));

            var envelopedEventHandlerInvocations =
                interfaces
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnvelopedEventHandler<>))
                    .Select(i => new { HandlerInterface = i, EventType = i.GetGenericArguments()[0] })
                    .Select(e => new Tuple<Type, Action<Envelope>>(e.EventType, this.BuildEnvelopeHandlerInvocation(handler, e.HandlerInterface, e.EventType)));

            return eventHandlerInvocations.Union(envelopedEventHandlerInvocations);
        }

        private Action<Envelope> BuildHandlerInvocation(IEventHandler handler, Type handlerType, Type messageType)
        {
            var envelopeType = typeof(Envelope<>).MakeGenericType(messageType);

            var parameter = Expression.Parameter(typeof(Envelope));
            var invocationExpression =
                Expression.Lambda(
                    Expression.Block(
                        Expression.Call(
                            Expression.Convert(Expression.Constant(handler), handlerType),
                            handlerType.GetMethod("Handle"),
                            Expression.Property(Expression.Convert(parameter, envelopeType), "Body"))),
                    parameter);

            return (Action<Envelope>)invocationExpression.Compile();
        }

        private Action<Envelope> BuildEnvelopeHandlerInvocation(IEventHandler handler, Type handlerType, Type messageType)
        {
            var envelopeType = typeof(Envelope<>).MakeGenericType(messageType);

            var parameter = Expression.Parameter(typeof(Envelope));
            var invocationExpression =
                Expression.Lambda(
                    Expression.Block(
                        Expression.Call(
                            Expression.Convert(Expression.Constant(handler), handlerType),
                            handlerType.GetMethod("Handle"),
                            Expression.Convert(parameter, envelopeType))),
                    parameter);

            return (Action<Envelope>)invocationExpression.Compile();
        }

        private Action<IEvent, string, string, string> BuildDispatchInvocation(Type eventType)
        {
            var eventParameter = Expression.Parameter(typeof(IEvent));
            var messageIdParameter = Expression.Parameter(typeof(string));
            var correlationIdParameter = Expression.Parameter(typeof(string));
            var traceIdParameter = Expression.Parameter(typeof(string));

            var dispatchExpression =
                Expression.Lambda(
                    Expression.Block(
                        Expression.Call(
                            Expression.Constant(this),
                            this.GetType().GetMethod("DoDispatchMessage", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(eventType),
                            Expression.Convert(eventParameter, eventType),
                            messageIdParameter,
                            correlationIdParameter,
                            traceIdParameter)),
                    eventParameter,
                    messageIdParameter,
                    correlationIdParameter,
                    traceIdParameter);

            return (Action<IEvent, string, string, string>)dispatchExpression.Compile();
        }

        /// <summary>
        /// Do not delete ! use by reflection above 
        /// </summary>
        private void DoDispatchMessage<T>(T @event, string messageId, string correlationId, string traceIdentifier)
            where T : IEvent
        {
            var envelope = Envelope.Create(@event);
            envelope.MessageId = messageId;
            envelope.CorrelationId = correlationId;

            List<Tuple<Type, Action<Envelope>>> handlers;
            if (this.handlersByEventType.TryGetValue(typeof(T), out handlers))
            {
                foreach (var handler in handlers)
                {
                    Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "Event{0} handled by {1}.", traceIdentifier, handler.Item1.FullName));
                    handler.Item2(envelope);
                }
            }
        }

        public IEnumerable<IEvent> Events { get { return this.events; } }

        public void Publish(Envelope<IEvent> @event)
        {
            this.events.Add(@event.Body);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Action<IEvent, string, string, string> dispatch;
                    if (this.dispatchersByEventType.TryGetValue(@event.Body.GetType(), out dispatch))
                    {
                        dispatch(@event.Body, @event.MessageId, @event.CorrelationId, string.Empty);
                    }
                    // Invoke also the generic handlers that have registered to handle IEvent directly.
                    if (this.dispatchersByEventType.TryGetValue(typeof (IEvent), out dispatch))
                    {
                        dispatch(@event.Body, @event.MessageId, @event.CorrelationId, string.Empty);
                    }

                }catch(Exception e)
                {
                    Trace.TraceError("Error in handling event " + @event.Body.GetType(), e);
                }
            });
        }

        public void Publish(IEnumerable<Envelope<IEvent>> events)
        {
            foreach (var @event in events)
            {
                this.Publish(@event);
            }
        }
    }
}

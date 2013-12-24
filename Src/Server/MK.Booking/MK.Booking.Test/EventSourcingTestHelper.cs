#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Extensions;
using Infrastructure;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test
{
    public class EventSourcingTestHelper<T> where T : IEventSourced
    {
        private readonly RepositoryStub _repository;
        private ICommandHandler _handler;

        static EventSourcingTestHelper()
        {
            new Module().RegisterMaps();
        }

        public EventSourcingTestHelper()
        {
            Events = new List<IVersionedEvent>();
            _repository = new RepositoryStub((eventSouced, correlationId) => Events.AddRange(eventSouced.Events));
        }

        public List<IVersionedEvent> Events { get; private set; }

        public IEventSourcedRepository<T> Repository
        {
            get { return _repository; }
        }

        public void Setup(ICommandHandler handler)
        {
            _handler = handler;
        }

        public void Given(params IVersionedEvent[] history)
        {
            var badEvent = history.FirstOrDefault(evnt => evnt.SourceId == default(Guid));
            if (badEvent != null)
            {
                throw new ArgumentException("Please set the source ID on your events, " + badEvent.GetType().Name);
            }
            _repository.History.AddRange(history);
        }

        public void When(ICommand command)
        {
            ((dynamic) _handler).Handle((dynamic) command);
        }

        public void When(IEvent @event)
        {
            ((dynamic) _handler).Handle((dynamic) @event);
        }

        public IEnumerable<TEvent> ThenHas<TEvent>() where TEvent : IVersionedEvent
        {
            var evts = Events.OfType<TEvent>().ToArray();
            if (!evts.Any())
            {
                throw new Exception("No events found");
            }
            return evts;
        }

        public bool ThenHasNo<TEvent>() where TEvent : IVersionedEvent
        {
            if (Events.OfType<TEvent>().IsEmpty())
            {
                return true;
            }
            throw new Exception("Events found for type " + typeof (TEvent));
        }

        public bool ThenContains<TEvent>() where TEvent : IVersionedEvent
        {
            return Events.Any(x => x.GetType() == typeof (TEvent));
        }

        public TEvent ThenHasSingle<TEvent>() where TEvent : IVersionedEvent
        {
            Assert.AreEqual(1, Events.Count);
            var @event = Events.Single();
            Assert.IsAssignableFrom<TEvent>(@event);
            return (TEvent) @event;
        }

        public TEvent ThenHasOne<TEvent>() where TEvent : IVersionedEvent
        {
            Assert.AreEqual(1, Events.OfType<TEvent>().Count());
            var @event = Events.OfType<TEvent>().Single();
            return @event;
        }

        private class RepositoryStub : IEventSourcedRepository<T>
        {
            public readonly List<IVersionedEvent> History = new List<IVersionedEvent>();
            private readonly Func<Guid, IEnumerable<IVersionedEvent>, T> _entityFactory;
            private readonly Action<T, string> _onSave;

            internal RepositoryStub(Action<T, string> onSave)
            {
                _onSave = onSave;
                var constructor = typeof (T).GetConstructor(new[] {typeof (Guid), typeof (IEnumerable<IVersionedEvent>)});
                if (constructor == null)
                {
                    throw new InvalidCastException(
                        "Type T must have a constructor with the following signature: .ctor(Guid, IEnumerable<IVersionedEvent>)");
                }
                _entityFactory = (id, events) => (T) constructor.Invoke(new object[] {id, events});
            }

            T IEventSourcedRepository<T>.Find(Guid id)
            {
                var all = History.Where(x => x.SourceId == id).ToList();
                if (all.Count > 0)
                {
                    return _entityFactory.Invoke(id, all);
                }

                return default(T);
            }

            T IEventSourcedRepository<T>.Get(Guid id)
            {
                var entity = ((IEventSourcedRepository<T>) this).Find(id);
                if (Equals(entity, default(T)))
                {
                    throw new EntityNotFoundException(id, "Test");
                }

                return entity;
            }

            public void Save(T eventSourced, string correlationId)
            {
                _onSave(eventSourced, correlationId);
            }
        }
    }
}
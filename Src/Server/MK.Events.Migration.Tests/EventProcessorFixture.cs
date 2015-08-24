using apcurium.MK.Booking.Events;
using apcurium.MK.Events.Migration;
using apcurium.MK.Events.Migration.Processor;
using NUnit.Framework;

namespace MK.Events.Migration.Tests
{
    [TestFixture]
    public class EventProcessorFixture
    {
        [Test]
        public void when_event_migrator_register_and_process_event_should_be_invoked()
        {
            var sut = new EventProcessor();
            var dummyMigrator = new DummyMigrator();
            sut.Register(dummyMigrator);

            var @event = sut.ProcessMessage(new AccountRegistered()) as AccountRegistered;

            Assert.IsTrue(dummyMigrator.Invoked);
            Assert.AreEqual(66, @event.NbPassengers);
        }

        [Test]
        public void when_events_migrators_register_and_process_event_should_be_invoked()
        {
            var sut = new EventProcessor();
            var dummyMigrator = new DummyMigrator();
            sut.Register(dummyMigrator);
            var dummyMigrator2 = new DummyMigrator2();
            sut.Register(dummyMigrator2);

            var @event = sut.ProcessMessage(new AccountRegistered()) as AccountRegistered;

            Assert.IsTrue(dummyMigrator.Invoked);
            Assert.IsTrue(dummyMigrator2.Invoked);
        }

        [Test]
        public void when_events_migrators_register_and_process_event_should_be_invoked_only_for_type_supported()
        {
            var sut = new EventProcessor();
            var dummyMigrator = new DummyMigrator();
            sut.Register(dummyMigrator);
            var dummyMigrator2 = new DummyMigrator2();
            sut.Register(dummyMigrator2);

           sut.ProcessMessage(new CreditCardDeactivated());

            Assert.IsFalse(dummyMigrator.Invoked);
            Assert.IsTrue(dummyMigrator2.Invoked);
        }
    }

    class DummyMigrator : IMigrateEvent<AccountRegistered>
    {
        public AccountRegistered Migrate(AccountRegistered @event)
        {
            @event.NbPassengers = 66;
            Invoked = true;
            return @event;
        }

        public bool Invoked { get; set; }
    }

    class DummyMigrator2 : IMigrateEvent<AccountRegistered>, IMigrateEvent<CreditCardDeactivated>
    {
        public AccountRegistered Migrate(AccountRegistered @event)
        {
            @event.NbPassengers = 66;
            Invoked = true;
            return @event;
        }

        public bool Invoked { get; set; }
        public CreditCardDeactivated Migrate(CreditCardDeactivated @event)
        {
            Invoked = true;
            return @event;
        }
    }
}
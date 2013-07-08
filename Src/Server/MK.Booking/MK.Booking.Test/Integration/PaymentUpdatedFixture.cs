using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Test.Integration
{

    [TestFixture]
    public class PaymentUpdatedFixture : given_a_config_read_model_database
    {
        protected PaymentSettingsUpdater sut;
        protected List<ICommand> commands = new List<ICommand>();

        [SetUp]
        public void Setup()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            commands = new List<ICommand>();

            this.sut = new PaymentSettingsUpdater(new AccountDao(() => new BookingDbContext(dbName)), bus.Object);
        }

        [Test]
        public void on_paymentmode_changed()
        {
            sut.Handle(new PaymentModeChanged());
            Assert.True(commands.Count == 1);
            var command = commands.Single();
            Assert.IsAssignableFrom<DeleteAllCreditCards>(command);
         }
    }
}

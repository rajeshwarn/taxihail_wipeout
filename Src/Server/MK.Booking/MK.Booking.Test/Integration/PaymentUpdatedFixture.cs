#region

using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.Integration
{
    [TestFixture]
    public class given_payment : given_a_config_read_model_database
    {
        [SetUp]
        public void Setup()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Commands = new List<ICommand>();

            Sut = new PaymentSettingsUpdater(new AccountDao(() => new BookingDbContext(DbName)), bus.Object);
        }

        protected PaymentSettingsUpdater Sut;
        protected List<ICommand> Commands = new List<ICommand>();

        [Test]
        public void on_paymentmode_changed()
        {
            Sut.Handle(new PaymentModeChanged());
            Assert.True(Commands.Count == 1);
            var command = Commands.Single();
            Assert.IsAssignableFrom<DeleteAllCreditCards>(command);
        }
    }
}
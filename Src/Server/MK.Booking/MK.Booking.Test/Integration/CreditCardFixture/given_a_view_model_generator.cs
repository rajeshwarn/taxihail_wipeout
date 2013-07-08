using System.Collections.Generic;
using System.Linq;
using Infrastructure.Messaging;
using Moq;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;

namespace apcurium.MK.Booking.Test.Integration.CreditCardFixture
{
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected CreditCardDetailsGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new CreditCardDetailsGenerator(() => new BookingDbContext(dbName));
        }
    }
}
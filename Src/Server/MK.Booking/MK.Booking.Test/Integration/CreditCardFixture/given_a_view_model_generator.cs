#region

using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using Infrastructure.Messaging;
using Moq;

#endregion

namespace apcurium.MK.Booking.Test.Integration.CreditCardFixture
{
// ReSharper disable once InconsistentNaming
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected CreditCardDetailsGenerator Sut;

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new CreditCardDetailsGenerator(() => new BookingDbContext(DbName));
        }
    }
}
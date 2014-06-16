using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Integration.OrderFixture
{
    
        public class given_a_order_user_generator : given_a_read_model_database
        {
            protected List<ICommand> Commands = new List<ICommand>();
            protected OrderUserGpsGenerator Sut;

            public given_a_order_user_generator()
            {
                var bus = new Mock<ICommandBus>();
                bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                    .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
                bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                    .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

                Sut = new OrderUserGpsGenerator(() => new BookingDbContext(DbName), new Logger());
            }
        }

        [TestFixture]
        public class given_no_order_user_gps : given_a_order_user_generator
        {
            [Test]
            public void when_order_created_then_order_dto_populated()
            {
                var orderId = Guid.NewGuid();
                
                Sut.Handle(new OrderCreated
                {
                    SourceId = orderId,
                    UserLatitude = 46.50643,
                    UserLongitude = -74.554052
                });

                using (var context = new BookingDbContext(DbName))
                {
                    var infos = context.Find<OrderUserGpsDetail>(orderId);
                    Assert.AreEqual(46.50643, infos.UserLatitude);
                    Assert.AreEqual(-74.554052, infos.UserLongitude);
                }
            }
        }
    
}
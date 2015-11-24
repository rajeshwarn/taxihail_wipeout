using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using CustomerPortal.Client.Impl;
using Infrastructure.Messaging;
using Infrastructure.Messaging.InMemory;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Integration.OrderFixture
{
    public class given_a_order_dispatch_company_manager : given_a_view_model_generator
    {
        private Guid _orderId;
        protected OrderDispatchCompanyManager Sut;

        [SetUp]
        public void Setup()
        {
            _orderId = Guid.NewGuid();

            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Commands = new List<ICommand>();

            Sut = new OrderDispatchCompanyManager(bus.Object,
                () => new BookingDbContext(DbName),
                new IBSServiceProvider(new TestServerSettings(), new Logger(), new TaxiHailNetworkServiceClient(new TestServerSettings())),
                new TaxiHailNetworkServiceClient(new TestServerSettings()),
                new ConfigurationDao(() => new ConfigurationDbContext(DbName)),
                new Logger(),
                new TestServerSettings());

            var ordetailsGenerator = new OrderGenerator(
                new EntityProjectionSet<OrderDetail>(() => new BookingDbContext(DbName)),
                new EntityProjectionSet<OrderStatusDetail>(() => new BookingDbContext(DbName)),
                new OrderRatingEntityProjectionSet(() => new BookingDbContext(DbName)),
                new EntityProjectionSet<OrderPairingDetail>(() => new BookingDbContext(DbName)),
                new EntityProjectionSet<OrderManualRideLinqDetail>(() => new BookingDbContext(DbName)),
                new EntityProjectionSet<OrderNotificationDetail>(() => new BookingDbContext(DbName)),
                new Logger(), 
                new TestServerSettings());
            ordetailsGenerator.Handle(new OrderCreated
            {
                SourceId = _orderId,
                AccountId = Guid.NewGuid(),
                UserLatitude = 45.463944,
                UserLongitude = -73.643234,
                PickupAddress = new Address
                {
                    Apartment = "3939",
                    Street = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = 45.463944,
                    Longitude = -73.643234
                },
                PickupDate = DateTime.Now,
                DropOffAddress = new Address
                {
                    Street = "Velvet auberge st gabriel",
                    Latitude = 45.411296,
                    Longitude = -73.613314,
                },
                CreatedDate = DateTime.Now,
            });
        }

        [Ignore("Too many components tested. Cannot fake companies in network from MKBooking.")]
        [Test]
        public void when_order_timed_out_then_dto_updated()
        {
            Sut.Handle(new OrderTimedOut
            {
                SourceId = _orderId
            });

            Thread.Sleep(500);

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderStatusDetail>(_orderId);
                Assert.NotNull(dto);
                Assert.AreEqual(OrderStatus.TimedOut, dto.Status);
                Assert.NotNull(dto.NextDispatchCompanyKey);
                Assert.NotNull(dto.NextDispatchCompanyName);
            }
        }
    }
}

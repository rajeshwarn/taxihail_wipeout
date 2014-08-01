using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Integration.CompanyFixture
{
    public class given_a_vehicle_type_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected VehicleTypeDetailGenerator Sut;

        public given_a_vehicle_type_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new VehicleTypeDetailGenerator(() => new BookingDbContext(DbName));
        }
    }

    [TestFixture]
    public class given_no_vehicle_type : given_a_vehicle_type_generator
    {
        [Test]
        public void when_vehicle_type_created_then_dto_populated()
        {

            var @event = new VehicleTypeAddedUpdated
            {
                SourceId = Guid.NewGuid(),
                VehicleTypeId = Guid.NewGuid(),
                LogoName = "taxi",
                Name = "Taxi",
                ReferenceDataVehicleId = 123
            };

            Sut.Handle(@event);

            using (var context = new BookingDbContext(DbName))
            {
                var list = context.Query<VehicleTypeDetail>().ToList();
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(@event.VehicleTypeId, dto.Id);
                Assert.AreEqual(@event.LogoName, dto.LogoName);
                Assert.AreEqual(@event.Name, dto.Name);
                Assert.AreEqual(@event.ReferenceDataVehicleId, dto.ReferenceDataVehicleId);
            }
        }
    }

    [TestFixture]
    public class given_a_vehicle_type : given_a_vehicle_type_generator
    {
        [SetUp]
        public void SetUp()
        {
            Sut.Handle(new VehicleTypeAddedUpdated
            {
                SourceId = Guid.NewGuid(),
                VehicleTypeId = _vehicleTypeId,
                LogoName = "taxi",
                Name = "Taxi",
                ReferenceDataVehicleId = 123
            });
        }

        private Guid _vehicleTypeId = Guid.NewGuid();
        private Guid _vehicleTypeId2 = Guid.NewGuid();

        [Test]
        public void when_vehicle_type_updated_then_dto_updated()
        {
            var @event = new VehicleTypeAddedUpdated
            {
                SourceId = Guid.NewGuid(),
                VehicleTypeId = _vehicleTypeId,
                LogoName = "taxi2",
                Name = "Taxi2",
                ReferenceDataVehicleId = 111
            };

            Sut.Handle(@event);

            using (var context = new BookingDbContext(DbName))
            {
                var list = context.Query<VehicleTypeDetail>().ToList();
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(@event.VehicleTypeId, dto.Id);
                Assert.AreEqual(@event.LogoName, dto.LogoName);
                Assert.AreEqual(@event.Name, dto.Name);
                Assert.AreEqual(@event.ReferenceDataVehicleId, dto.ReferenceDataVehicleId);
            }
        }

        [Test]
        public void when_adding_multiple_vehicle_types()
        {
            var @vehicleEvent1 = new VehicleTypeAddedUpdated
            {
                SourceId = Guid.NewGuid(),
                VehicleTypeId = _vehicleTypeId,
                LogoName = "taxi1",
                Name = "Taxi1",
                ReferenceDataVehicleId = 111
            };
            
            var @vehicleEvent2 = new VehicleTypeAddedUpdated
            {
                SourceId = Guid.NewGuid(),
                VehicleTypeId = _vehicleTypeId2,
                LogoName = "taxi2",
                Name = "Taxi2",
                ReferenceDataVehicleId = 10
            };

            Sut.Handle(@vehicleEvent1);
            Sut.Handle(@vehicleEvent2);

            using (var context = new BookingDbContext(DbName))
            {
                var vehicleTypes = context.Query<VehicleTypeDetail>().OrderBy(x => x.CreatedDate).ToList();

                Assert.AreEqual(2, vehicleTypes.Count());
                
                var firstDto = vehicleTypes.FirstOrDefault();
                Assert.IsNotNull(firstDto);
                Assert.AreEqual(@vehicleEvent1.Name, firstDto.Name);
                
                var secondDto = vehicleTypes.LastOrDefault();
                Assert.IsNotNull(secondDto);
                Assert.AreEqual(@vehicleEvent2.Name, secondDto.Name);
            }
        }

        [Test]
        public void when_vehicle_type_deleted_then_dto_removed()
        {
            Sut.Handle(new VehicleTypeDeleted
            {
                VehicleTypeId = _vehicleTypeId
            });
            Sut.Handle(new VehicleTypeDeleted
            {
                VehicleTypeId = _vehicleTypeId2
            });


            using (var context = new BookingDbContext(DbName))
            {
                var list = context.Query<VehicleTypeDetail>().ToList();
                Assert.AreEqual(0, list.Count());
            }
        }
    }
}
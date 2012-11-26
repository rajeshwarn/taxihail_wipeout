using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Test.Integration.CompanyFixture
{
    public class given_a_rating_type_view_model_generator : given_a_read_model_database
    {
        protected RatingTypeDetailsGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_rating_type_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new RatingTypeDetailsGenerator(() => new BookingDbContext(dbName));
        }
    }

    [TestFixture]
    public class given_no_ratingType : given_a_rating_type_view_model_generator
    {
        [Test]
        public void when_ratingType_created_ratingType_dto_populated()
        {
            var ratingTypeId = Guid.NewGuid();
            var companyId = Guid.NewGuid();

            var ratingTypeAdded = new RatingTypeAdded
                        {
                            SourceId = companyId,
                            RatingTypeId = ratingTypeId,
                            Name = "RatingType"
                        };

            this.sut.Handle(ratingTypeAdded);

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<RatingTypeDetail>().Where(x => x.Id == ratingTypeId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(ratingTypeId, dto.Id);
                Assert.AreEqual(companyId, dto.CompanyId);
                Assert.That(dto.Name, Is.EqualTo(ratingTypeAdded.Name));
            }
        }
    }

    [TestFixture]
    public class given_a_ratingType : given_a_rating_type_view_model_generator
    {
        private Guid _companyId;
        private Guid _ratingTypeId;

        [SetUp]
        public void SetUp()
        {

            this.sut.Handle(new RatingTypeAdded
                                {
                                    SourceId = (_companyId = Guid.NewGuid()),
                                    RatingTypeId = (_ratingTypeId = Guid.NewGuid()),
                                    Name = "RatingType " + Guid.NewGuid()
                                });
        }


        [Test]
        public void when_ratingType_updated_then_dto_updated()
        {
            var ratingTypeUpdated = new RatingTypeUpdated
                                        {
                                            SourceId = _companyId,
                                            RatingTypeId = _ratingTypeId,
                                            Name = "Updated RatingType",
                                        };

            this.sut.Handle(ratingTypeUpdated);

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<RatingTypeDetail>().Where(x => x.Id == _ratingTypeId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(_ratingTypeId, dto.Id);
                Assert.AreEqual(_companyId, dto.CompanyId);
                Assert.That(dto.Name, Is.EqualTo(ratingTypeUpdated.Name));
            }
        }

        [Test]
        public void when_ratingType_hidden()
        {
            this.sut.Handle(new RatingTypeHidded()
            {
                SourceId = _companyId,
                RatingTypeId = _ratingTypeId
            });

            using (var context = new BookingDbContext(dbName))
            {
                var ratingType = context.Query<RatingTypeDetail>().SingleOrDefault(x => x.Id == _ratingTypeId);

                Assert.That(ratingType, Is.Not.Null);
                Assert.That(ratingType.IsHidden, Is.True);
            }
        }
    }
}

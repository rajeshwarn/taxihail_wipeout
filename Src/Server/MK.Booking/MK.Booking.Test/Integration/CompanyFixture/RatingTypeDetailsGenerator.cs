﻿#region

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

#endregion

namespace apcurium.MK.Booking.Test.Integration.CompanyFixture
{
// ReSharper disable once InconsistentNaming
    public class given_a_rating_type_view_model_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected RatingTypeDetailsGenerator Sut;

        public given_a_rating_type_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new RatingTypeDetailsGenerator(() => new BookingDbContext(DbName));
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

            Sut.Handle(ratingTypeAdded);

            using (var context = new BookingDbContext(DbName))
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
        [SetUp]
        public void SetUp()
        {
            Sut.Handle(new RatingTypeAdded
            {
                SourceId = (_companyId = Guid.NewGuid()),
                RatingTypeId = (_ratingTypeId = Guid.NewGuid()),
                Name = "RatingType " + Guid.NewGuid()
            });
        }

        private Guid _companyId;
        private Guid _ratingTypeId;

        [Test]
        public void when_ratingType_hidden()
        {
            Sut.Handle(new RatingTypeHidded
            {
                SourceId = _companyId,
                RatingTypeId = _ratingTypeId
            });

            using (var context = new BookingDbContext(DbName))
            {
                var ratingType = context.Query<RatingTypeDetail>().SingleOrDefault(x => x.Id == _ratingTypeId);

                Assert.That(ratingType, Is.Not.Null);
// ReSharper disable once PossibleNullReferenceException
                Assert.That(ratingType.IsHidden, Is.True);
            }
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

            Sut.Handle(ratingTypeUpdated);

            using (var context = new BookingDbContext(DbName))
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
        public void when_ratingType_with_same_name_created()
        {
            using (var context = new BookingDbContext(DbName))
            {
                var firstRatingType = context.Query<RatingTypeDetail>().FirstOrDefault();

                if (firstRatingType != null)
                {
                    Sut.Handle(new RatingTypeAdded
                    {
                        SourceId = (_companyId = Guid.NewGuid()),
                        RatingTypeId = (_ratingTypeId = Guid.NewGuid()),
                        Name = firstRatingType.Name
                    });

                    var countWithName = context.Query<RatingTypeDetail>().Count(x => x.Name == firstRatingType.Name);
                    Assert.That(countWithName, Is.EqualTo(1));
                }
            }
        }
    }
}
using System;
using System.Linq;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class given_a_rating_type
    {
        private EventSourcingTestHelper<Company> sut;
        readonly Guid _companyId = Guid.NewGuid();
        readonly Guid _ratingTypeId = Guid.NewGuid();

        [SetUp]
        public void given_a_rating_type_setup()
        {
            this.sut = new EventSourcingTestHelper<Company>();
            this.sut.Setup(new CompanyCommandHandler(sut.Repository));
            this.sut.Given(new CompanyCreated { SourceId = _companyId });
            this.sut.Given(new RatingTypeAdded { SourceId = _companyId });
        }

        [Test]
        public void when_updating_rating_type()
        {
            sut.When(new UpdateRatingType
                         {
                             CompanyId = _companyId,
                             RatingTypeId = _ratingTypeId,
                             Name = "Updated"
                         });

            Assert.AreEqual(1, sut.Events.Count);
            Assert.IsInstanceOf<RatingTypeUpdated>(sut.Events.Single());
            var evt = (RatingTypeUpdated)sut.Events.Single();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(_ratingTypeId, evt.RatingTypeId);
            Assert.AreEqual("Updated", evt.Name);
        }

        [Test]
        public void when_hidding_rating_type()
        {
            sut.When(new HideRatingType { CompanyId = _companyId, RatingTypeId = _ratingTypeId });
            Assert.AreEqual(1, sut.Events.Count);
            Assert.IsInstanceOf<RatingTypeHidded>(sut.Events.Single());
            var evt = (RatingTypeHidded)sut.Events.Single();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(_ratingTypeId, evt.RatingTypeId);
        }
    }
}
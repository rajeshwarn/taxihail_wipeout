#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class given_a_company_with_a_rating_type
    {
        [SetUp]
        public void given_a_rating_type_setup()
        {
            _sut = new EventSourcingTestHelper<Company>();
            _sut.Setup(new CompanyCommandHandler(_sut.Repository));
            _sut.Given(new CompanyCreated {SourceId = _companyId});
            _sut.Given(new RatingTypeAdded {SourceId = _companyId});
        }

        private EventSourcingTestHelper<Company> _sut;
        private readonly Guid _companyId = Guid.NewGuid();
        private readonly Guid _ratingTypeId = Guid.NewGuid();

        [Test]
        public void when_hidding_rating_type()
        {
            _sut.When(new HideRatingType {CompanyId = _companyId, RatingTypeId = _ratingTypeId});

            var evt = _sut.ThenHasSingle<RatingTypeHidded>();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(_ratingTypeId, evt.RatingTypeId);
        }

        [Test]
        public void when_updating_rating_type()
        {
            _sut.When(new UpdateRatingType
            {
                CompanyId = _companyId,
                RatingTypeId = _ratingTypeId,
                Name = "Updated"
            });

            var evt = _sut.ThenHasSingle<RatingTypeUpdated>();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(_ratingTypeId, evt.RatingTypeId);
            Assert.AreEqual("Updated", evt.Name);
        }
    }
}
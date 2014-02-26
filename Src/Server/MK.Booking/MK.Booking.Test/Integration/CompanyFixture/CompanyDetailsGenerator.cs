using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.Test.Integration.CompanyFixture
{
    public class given_a_company_view_model_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected CompanyDetailsGenerator Sut;

        public given_a_company_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new CompanyDetailsGenerator(() => new BookingDbContext(DbName));
        }
    }

    [TestFixture]
    public class given_no_company : given_a_company_view_model_generator
    {
        [Test]
        public void when_company_created_company_dto_populated()
        {
            var companyId = Guid.NewGuid();

            Sut.Handle(new CompanyCreated
            {
                SourceId = companyId
            });

            using (var context = new BookingDbContext(DbName))
            {
                var list = context.Query<CompanyDetail>().Where(x => x.Id == companyId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(companyId, dto.Id);
                Assert.IsNullOrEmpty(dto.TermsAndConditions);
            }
        }
    }

    [TestFixture]
    public class given_existing_company : given_a_company_view_model_generator
    {
        private readonly Guid _companyId = Guid.NewGuid();

        public given_existing_company()
        {
            Sut.Handle(new CompanyCreated
            {
                SourceId = _companyId
            });
        }

        [Test]
        public void when_terms_and_conditions_updated_company_dto_updated()
        {
            const string termsAndConditions = "test terms and conditions";

            Sut.Handle(new TermsAndConditionsUpdated
            {
                SourceId = _companyId,
                TermsAndConditions = termsAndConditions,
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Set<CompanyDetail>().Find(_companyId);

                Assert.NotNull(dto);
                Assert.AreEqual(termsAndConditions, dto.TermsAndConditions);
            }
        }
    }
}
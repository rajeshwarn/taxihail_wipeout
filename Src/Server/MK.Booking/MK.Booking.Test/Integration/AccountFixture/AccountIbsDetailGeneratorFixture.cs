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

namespace apcurium.MK.Booking.Test.Integration.AccountFixture
{
    public class given_accountibsdetails_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected AccountIbsDetailGenerator Sut;

        public given_accountibsdetails_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new AccountIbsDetailGenerator(() => new BookingDbContext(DbName));
        }
    }
    [TestFixture]
    public class given_exsiting_account : given_accountibsdetails_generator
    {
        private readonly Guid _accountId = Guid.NewGuid();

        [Test]
        public void when_account_linked_to_home_ibs_then_dto_not_populated()
        {
            Sut.Handle(new AccountLinkedToIbs
            {
                SourceId = _accountId,
                IbsAccountId = 122
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<AccountIbsDetail>(_accountId);
                Assert.Null(dto);
            }
        }

        [Test]
        public void when_account_linked_to_roaming_ibs_then_dto_populated()
        {
            Sut.Handle(new AccountLinkedToIbs
            {
                SourceId = _accountId,
                IbsAccountId = 555,
                CompanyKey = "test"
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Query<AccountIbsDetail>().FirstOrDefault(x => x.AccountId == _accountId && x.CompanyKey == "test");

                Assert.NotNull(dto);
                Assert.AreEqual(555, dto.IBSAccountId);
            }
        }

        [Test]
        public void when_account_unlinked_from_ibs_then_dto_removed()
        {
            Sut.Handle(new AccountLinkedToIbs
            {
                SourceId = _accountId,
                IbsAccountId = 122
            });

            Sut.Handle(new AccountLinkedToIbs
            {
                SourceId = _accountId,
                IbsAccountId = 555,
                CompanyKey = "test"
            });

            Sut.Handle(new AccountLinkedToIbs
            {
                SourceId = _accountId,
                IbsAccountId = 556,
                CompanyKey = "test2"
            });

            Sut.Handle(new AccountUnlinkedFromIbs
            {
                SourceId = _accountId
            });

            using (var context = new BookingDbContext(DbName))
            {
                var accountIbsDetail = context.Query<AccountIbsDetail>().Where(x => x.AccountId == _accountId && x.CompanyKey == "test").ToList();
                Assert.IsEmpty(accountIbsDetail);
            }
        }
    }
}
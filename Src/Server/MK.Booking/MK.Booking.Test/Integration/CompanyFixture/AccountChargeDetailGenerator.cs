#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.Integration.CompanyFixture
{
// ReSharper disable once InconsistentNaming
    public class given_a_account_charge_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected AccountChargeDetailGenerator Sut;

        public given_a_account_charge_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new AccountChargeDetailGenerator(() => new BookingDbContext(DbName));
        }
    }

    [TestFixture]
    public class given_no_account_charge : given_a_account_charge_generator
    {
        [Test]
        public void when_account_charge_created_then_dto_populated()
        {
            
            var @event = new AccountChargeAddedUpdated
            {
                SourceId = Guid.NewGuid(),
                AccountChargeId = Guid.NewGuid(),
                Number = "number",
                Name = "VIP",
                Questions = new[]
                {
                    new AccountChargeQuestion
                    {
                        Answer = "answer",
                        Id = 0,
                        Question = "question"
                    }
                }
            };

            Sut.Handle(@event);

            using (var context = new BookingDbContext(DbName))
            {
                var list = context.Query<AccountChargeDetail>().ToList();
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(@event.AccountChargeId, dto.Id);
                Assert.AreEqual(@event.Number, dto.Number);
                Assert.AreEqual(@event.Name, dto.Name);
                Assert.AreEqual(@event.Questions[0].Question, dto.Questions[0].Question);
                Assert.AreEqual(@event.Questions[0].Answer, dto.Questions[0].Answer);
            }
        }
    }

    [TestFixture]
    public class given_an_account_charge : given_a_account_charge_generator
    {
        [SetUp]
        public void SetUp()
        {
            Sut.Handle(new AccountChargeAddedUpdated
            {
                SourceId = Guid.NewGuid(),
                AccountChargeId = _accountChargeId,
                Number = "number",
                Name = "VIP",
                Questions = new[]
                {
                    new AccountChargeQuestion
                    {
                        Answer = "answer",
                        Id = _questionId,
                        Question = "question"
                    }
                }
            });
        }

        private Guid _accountChargeId = Guid.NewGuid();
        private int _questionId = 1;

        [Test]
        public void when_account_charge_updated_then_dto_updated()
        {
            var @event = new AccountChargeAddedUpdated
            {
                SourceId = Guid.NewGuid(),
                AccountChargeId = _accountChargeId,
                Number = "number2",
                Name = "VIP2",
                Questions = new[]
                {
                    new AccountChargeQuestion
                    {
                        Answer = "answer2",
                        Id =_questionId,
                        Question = "question2"
                    }
                }
            };

            Sut.Handle(@event);

            using (var context = new BookingDbContext(DbName))
            {
                var list = context.Query<AccountChargeDetail>().ToList();
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(@event.AccountChargeId, dto.Id);
                Assert.AreEqual(@event.Number, dto.Number);
                Assert.AreEqual(@event.Name, dto.Name);
                Assert.AreEqual(@event.Questions[0].Question, dto.Questions[0].Question);
                Assert.AreEqual(@event.Questions[0].Answer, dto.Questions[0].Answer);
            }
        }

        [Test]
        public void when_account_charge_deleted_then_dto_removed()
        {


            Sut.Handle(new AccountChargeDeleted
            {
                AccountChargeId = _accountChargeId
            });
        

            using (var context = new BookingDbContext(DbName))
            {
                var list = context.Query<AccountChargeDetail>().ToList();
                Assert.AreEqual(0, list.Count());
            }
        }

    }
}
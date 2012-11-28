using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.Test.Integration.CreditCardFixture
{
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected CreditCardDetailsGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new CreditCardDetailsGenerator(() => new BookingDbContext(dbName));
        }
    }

    [TestFixture]
    public class given_no_account : given_a_view_model_generator
    {
        [Test]
        public void when_creditcard_added_then_creditcard_dto_populated()
        {
            var accountId = Guid.NewGuid();
            const string creditCardComapny = "visa";
            const string friendlyName = "work credit card";
            var creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";
            const string token = "jjwcnSLWm85";

            this.sut.Handle(new CreditCardAdded
                                {
                                    SourceId = accountId,
                                    CreditCardCompany = creditCardComapny,
                                    FriendlyName = friendlyName,
                                    CreditCardId = creditCardId,
                                    Last4Digits = last4Digits,
                                    Token = token
                                });

            using (var context = new BookingDbContext(dbName))
            {
                var dto = context.Find<CreditCardDetails>(creditCardId);
                Assert.NotNull(dto);
                Assert.AreEqual(accountId, dto.AccountId);
                Assert.AreEqual(creditCardComapny, dto.CreditCardCompany);
                Assert.AreEqual(friendlyName, dto.FriendlyName);
                Assert.AreEqual(creditCardId, dto.CreditCardId);
                Assert.AreEqual(last4Digits, dto.Last4Digits);
                Assert.AreEqual(token, dto.Token);
            }
        }
    }
}
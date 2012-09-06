using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.BackOffice.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;

namespace apcurium.MK.Booking.Test.AddressesFixture
{
    [TestFixture]
    public class given_no_address
    {
        private EventSourcingTestHelper<Account> sut;
        private readonly Guid _accountId = Guid.NewGuid();

        public given_no_address()
        {
            this.sut = new EventSourcingTestHelper<Account>();
            this.sut.Setup(new AddressCommandHandler(this.sut.Repository));
            this.sut.Given(new AccountRegistered { SourceId = _accountId, Name = "Bob", Password = null, Email = "bob.smith@apcurium.com" });
        }

        [Test]
        public void when_adding_an_address_successfully()
        {
            var addressId = Guid.NewGuid();
            this.sut.When(new AddFavoriteAddress { AddressId = addressId, AccountId =  _accountId, FriendlyName = "Chez François", Apartment = "3939", FullAddress = "1234 rue Saint-Hubert", RingCode = "3131", Latitude   = 45.515065, Longitude = -73.558064 });

            Assert.AreEqual(1, sut.Events.Count);
            var evt = (FavoriteAddressAdded)sut.Events.Single();
            Assert.AreEqual(_accountId, evt.SourceId);
            Assert.AreEqual(addressId, evt.AddressId);
            Assert.AreEqual("Chez François", evt.FriendlyName);
            Assert.AreEqual("3939", evt.Apartment);
            Assert.AreEqual("1234 rue Saint-Hubert", evt.FullAddress);
            Assert.AreEqual("3131", evt.RingCode);
            Assert.AreEqual(45.515065, evt.Latitude);
            Assert.AreEqual(-73.558064, evt.Longitude);

        }

        [Test]
        public void when_adding_an_address_with_missing_required_fields()
        {
            Assert.Throws<InvalidOperationException>(() => this.sut.When(new AddFavoriteAddress { AccountId = _accountId, FriendlyName = null, Apartment = "3939", FullAddress = null, RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064 }));
        }

        [Test]
        public void when_adding_an_address_with_and_invalid_latitude_or_longitude()
        {

            Assert.Throws<ArgumentOutOfRangeException>(() => this
                .sut.When(new AddFavoriteAddress
                    {
                        AccountId = _accountId,
                        FriendlyName = "Chez François",
                        Apartment = "3939",
                        FullAddress = "1234 rue Saint-Hubert",
                        RingCode = "3131",
                        Latitude = 180,
                        Longitude = -73.558064
                    }));

            Assert.Throws<ArgumentOutOfRangeException>(() => this
                .sut.When(new AddFavoriteAddress
                {
                    AccountId = _accountId,
                    FriendlyName = "Chez François",
                    Apartment = "3939",
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = 0,
                    Longitude = -200.558064
                }));
        }

        
    }
}

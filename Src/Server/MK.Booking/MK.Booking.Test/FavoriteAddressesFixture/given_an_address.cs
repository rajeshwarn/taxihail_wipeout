using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using apcurium.MK.Booking.BackOffice.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;

namespace BackOffice.Test.FavoriteAddressesFixture
{
    public class given_an_address
    {
        private EventSourcingTestHelper<Account> sut;
        private Guid _accountId = Guid.NewGuid();

        public given_an_address()
        {
            this.sut = new EventSourcingTestHelper<Account>();
            this.sut.Setup(new FavoriteAddressCommandHandler(this.sut.Repository));
            this.sut.Given(new AccountRegistered { SourceId = _accountId, FirstName = "Bob", LastName = "Smith", Password = "bsmith", Email = "bob.smith@apcurium.com" });
        }

        [Fact]
        public void when_adding_address_successfully()
        {
            this.sut.When(new AddFavoriteAddress { AccountId =  _accountId, FriendlyName = "Chez François", Apartment = "3939", FullAddress = "1234 rue Saint-Hubert", RingCode = "3131", Latitude   = 45.515065, Longitude = -73.558064 });

            Assert.Single(sut.Events);
            var evt = (FavoriteAddressAdded)sut.Events.Single();
            Assert.Equal(_accountId, evt.SourceId);
            Assert.Equal("Chez François", evt.FriendlyName);
            Assert.Equal("3939", evt.Apartment);
            Assert.Equal("1234 rue Saint-Hubert", evt.FullAddress);
            Assert.Equal("3131", evt.RingCode);
            Assert.Equal(45.515065, evt.Latitude);
            Assert.Equal(-73.558064, evt.Longitude);

        }

        [Fact]
        public void when_adding_an_address_with_missing_required_fields()
        {
            Assert.Throws<InvalidOperationException>(() => this.sut.When(new AddFavoriteAddress { AccountId = _accountId, FriendlyName = null, Apartment = "3939", FullAddress = null, RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064 }));
        }

        [Fact]
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

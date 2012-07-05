﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using Moq;
using Xunit;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.BackOffice.EventHandlers;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Test.Integration;

namespace BackOffice.Test.Integration.FavoriteAddressFixture
{
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected FavoriteAddressListGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new FavoriteAddressListGenerator(() => new BookingDbContext(dbName));
        }
    }

    public class given_no_address : given_a_view_model_generator
    {
        [Fact]
        public void when_address_is_added_to_favorites_then_list_updated()
        {
            var accountId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            this.sut.Handle(new FavoriteAddressAdded
            {
                SourceId = accountId,
                AddressId = addressId,
                FriendlyName = "Chez François",
                Apartment = "3939",
                FullAddress = "1234 rue Saint-Hubert",
                RingCode = "3131",
                Latitude = 45.515065,
                Longitude = -73.558064
            });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<FavoriteAddress>().Where(x => x.AccountId == accountId);

                Assert.Single(list);
                var dto = list.Single();
                Assert.Equal(addressId, dto.Id);
                Assert.Equal(accountId, dto.AccountId);
                Assert.Equal("Chez François", dto.FriendlyName);
                Assert.Equal("3939", dto.Apartment);
                Assert.Equal("1234 rue Saint-Hubert", dto.FullAddress);
                Assert.Equal("3131", dto.RingCode);
                Assert.Equal(45.515065, dto.Latitude);
                Assert.Equal(-73.558064, dto.Longitude);
            }
        }
    }

    public class given_an_address : given_a_view_model_generator
    {
        private readonly Guid _accountId = Guid.NewGuid();
        private readonly Guid _addressId;
        public given_an_address()
        {
            sut.Handle(new FavoriteAddressAdded
            {
                AddressId = Guid.NewGuid(),
                SourceId = _accountId,
                FriendlyName = "Chez François",
                Apartment = "3939",
                FullAddress = "1234 rue Saint-Hubert",
                RingCode = "3131",
                Latitude = 45.515065,
                Longitude = -73.558064
            });

            using (var context = new BookingDbContext(dbName))
            {
                _addressId = context.Query<FavoriteAddress>().Single().Id;
            }
        }

        [Fact]
        public void when_address_is_removed_from_favorites_then_list_updated()
        {
            this.sut.Handle(new FavoriteAddressRemoved
            {
                SourceId = _accountId,
                AddressId = _addressId
            });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<FavoriteAddress>();
                Assert.Empty(list);
            }
        }

        [Fact]
        public void when_address_is_updated_successfully()
        {
            this.sut.Handle(new FavoriteAddressUpdated
            {
                SourceId = _accountId,
                AddressId = _addressId,
                FriendlyName = "Chez Costo !",
                FullAddress = "25 rue Berri Montreal"
            });

            using (var context = new BookingDbContext(dbName))
            {
                var address = context.Find<FavoriteAddress>(_addressId);
                Assert.NotNull(address);
                Assert.Equal("25 rue Berri Montreal", address.FullAddress);
                Assert.Equal("Chez Costo !", address.FriendlyName);
                Assert.Null(address.RingCode);
                Assert.Null(address.Apartment);
                Assert.Equal(0,address.Latitude);
                Assert.Equal(0, address.Longitude);
            }
        }
    }
}

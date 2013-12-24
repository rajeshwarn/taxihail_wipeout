using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Test.Integration;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Test.Integration.FavoriteAddressFixture
{

    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected AddressListGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new AddressListGenerator(() => new BookingDbContext(dbName));
        }
    }

    [TestFixture]
    public class given_no_address : given_a_view_model_generator
    {
        [Test]
        public void when_address_is_added_to_favorites_then_list_updated()
        {
            var accountId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            this.sut.Handle(new FavoriteAddressAdded
            {
                SourceId = accountId,
                Address = new Address
                {
                    Id = addressId,
                    FriendlyName = "Chez François",
                    Apartment = "3939",
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    BuildingName = "Hôtel de Ville",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                }
            });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<AddressDetails>().Where(x => x.AccountId == accountId);

                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(addressId, dto.Id);
                Assert.AreEqual(accountId, dto.AccountId);
                Assert.AreEqual("Chez François", dto.FriendlyName);
                Assert.AreEqual("3939", dto.Apartment);
                Assert.AreEqual("1234 rue Saint-Hubert", dto.FullAddress);
                Assert.AreEqual("3131", dto.RingCode);
                Assert.AreEqual("Hôtel de Ville", dto.BuildingName);
                Assert.AreEqual(45.515065, dto.Latitude);
                Assert.AreEqual(-73.558064, dto.Longitude);
            }
        }

        [Test]
        public void when_address_is_added_to_companydefaultfavorites_then_list_updated()
        {
            var addressId = Guid.NewGuid();

            this.sut.Handle(new DefaultFavoriteAddressAdded
            {
                Address = new Address
                   {
                       Id = addressId,
                       FriendlyName = "Chez François",
                       Apartment = "3938",
                       FullAddress = "1234 rue Saint-Hubert",
                       RingCode = "3131",
                       BuildingName = "Hôtel de Ville",
                       Latitude = 45.515065,
                       Longitude = -73.558064
                   }
            });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<DefaultAddressDetails>().Where(x => x.Id == addressId);

                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(addressId, dto.Id);
                Assert.AreEqual("Chez François", dto.FriendlyName);
                Assert.AreEqual("3938", dto.Apartment);
                Assert.AreEqual("1234 rue Saint-Hubert", dto.FullAddress);
                Assert.AreEqual("3131", dto.RingCode);
                Assert.AreEqual("Hôtel de Ville", dto.BuildingName);
                Assert.AreEqual(45.515065, dto.Latitude);
                Assert.AreEqual(-73.558064, dto.Longitude);
            }
        }

        [Test]
        public void when_address_is_added_to_company_popular_then_list_updated()
        {
            var addressId = Guid.NewGuid();

            this.sut.Handle(new PopularAddressAdded
            {
                Address = new Address
                             {
                                 Id = addressId,
                                 FriendlyName = "Chez François popular",
                                 Apartment = "3938",
                                 FullAddress = "1234 rue Saint-Hubert",
                                 RingCode = "3131",
                                 BuildingName = "Hôtel de Ville",
                                 Latitude = 45.515065,
                                 Longitude = -73.558064
                             }
            });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<PopularAddressDetails>().Where(x => x.Id == addressId);

                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(addressId, dto.Id);
                Assert.AreEqual("Chez François popular", dto.FriendlyName);
                Assert.AreEqual("3938", dto.Apartment);
                Assert.AreEqual("1234 rue Saint-Hubert", dto.FullAddress);
                Assert.AreEqual("3131", dto.RingCode);
                Assert.AreEqual("Hôtel de Ville", dto.BuildingName);
                Assert.AreEqual(45.515065, dto.Latitude);
                Assert.AreEqual(-73.558064, dto.Longitude);
            }
        }
    }

    [TestFixture]
    public class given_an_address : given_a_view_model_generator
    {
        private readonly Guid _accountId = Guid.NewGuid();
        private Guid _addressId;
        private Guid _companyDefaultAddressId;
        private Guid _popularAddressId;

        [SetUp]
        public void Setup()
        {
            _addressId = Guid.NewGuid();
            _popularAddressId = Guid.NewGuid();
            _companyDefaultAddressId = Guid.NewGuid();
            sut.Handle(new FavoriteAddressAdded
                           {
                               SourceId = _accountId,
                               Address = new Address
                                {
                                    Id = _addressId,
                                    FriendlyName = "Chez François",
                                    Apartment = Guid.NewGuid().ToString(),
                                    FullAddress = "1234 rue Saint-Hubert",
                                    RingCode = "3131",
                                    Latitude = 45.515065,
                                    Longitude = -73.558064
                                }
                           });

            sut.Handle(new DefaultFavoriteAddressAdded
            {
                Address = new Address
                {
                    Id = _addressId,
                    FriendlyName = "Chez François",
                    Apartment = Guid.NewGuid().ToString(),
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                }
            });

            sut.Handle(new PopularAddressAdded
            {
                Address = new Address
                             {
                                 Id = _popularAddressId,
                                 FriendlyName = "Chez François popular",
                                 Apartment = Guid.NewGuid().ToString(),
                                 FullAddress = "1234 rue Saint-Hubert",
                                 RingCode = "3131",
                                 Latitude = 45.515065,
                                 Longitude = -73.558064
                             }
            });

        }

        [Test]
        public void when_address_is_removed_from_favorites_then_list_updated()
        {
            this.sut.Handle(new FavoriteAddressRemoved
                                {
                                    SourceId = _accountId,
                                    AddressId = _addressId
                                });

            using (var context = new BookingDbContext(dbName))
            {
                var address = context.Find<AddressDetails>(_addressId);
                Assert.IsNull(address);
            }
        }

        [Test]
        public void when_company_default_address_is_removed_from_favorites_then_list_updated()
        {
            this.sut.Handle(new DefaultFavoriteAddressRemoved
            {
                AddressId = _addressId
            });

            using (var context = new BookingDbContext(dbName))
            {
                var address = context.Find<DefaultAddressDetails>(_addressId);
                Assert.IsNull(address);
            }
        }

        [Test]
        public void when_company_popular_address_is_removed_from_favorites_then_list_updated()
        {
            this.sut.Handle(new PopularAddressRemoved
            {
                AddressId = _addressId
            });

            using (var context = new BookingDbContext(dbName))
            {
                var address = context.Find<PopularAddressDetails>(_addressId);
                Assert.IsNull(address);
            }
        }

        [Test]
        public void when_address_is_updated_successfully()
        {
            this.sut.Handle(new FavoriteAddressUpdated
                                {
                                    SourceId = _accountId,
                                    Address = new Address
               {
                   Id = _addressId,
                   FriendlyName = "Chez Costo !",
                   FullAddress = "25 rue Berri Montreal",
                   BuildingName = "Hôtel de Ville",
               }
                                });

            using (var context = new BookingDbContext(dbName))
            {
                var address = context.Find<AddressDetails>(_addressId);

                Assert.NotNull(address);
                Assert.AreEqual("25 rue Berri Montreal", address.FullAddress);
                Assert.AreEqual("Chez Costo !", address.FriendlyName);
                Assert.Null(address.RingCode);
                Assert.Null(address.Apartment);
                Assert.AreEqual("Hôtel de Ville", address.BuildingName);
                Assert.AreEqual(0, address.Latitude);
                Assert.AreEqual(0, address.Longitude);
            }
        }

        [Test]
        public void when_company_default_address_is_updated_successfully()
        {
            this.sut.Handle(new DefaultFavoriteAddressUpdated
            {
                Address = new Address
                {
                    Id = _addressId,
                    FriendlyName = "Chez Costo2 !",
                    FullAddress = "25 rue Berri Montreal",
                    BuildingName = "Hôtel de Ville",
                }
            });

            using (var context = new BookingDbContext(dbName))
            {
                var address = context.Find<DefaultAddressDetails>(_addressId);

                Assert.NotNull(address);
                Assert.AreEqual("25 rue Berri Montreal", address.FullAddress);
                Assert.AreEqual("Chez Costo2 !", address.FriendlyName);
                Assert.Null(address.RingCode);
                Assert.Null(address.Apartment);
                Assert.AreEqual("Hôtel de Ville", address.BuildingName);
                Assert.AreEqual(0, address.Latitude);
                Assert.AreEqual(0, address.Longitude);
            }
        }

        [Test]
        public void when_company_popular_address_is_updated_successfully()
        {
            this.sut.Handle(new PopularAddressUpdated
            {
                Address = new Address
                             {
                                 Id = _popularAddressId,
                                FriendlyName = "Chez Costo2 popular !",
                                FullAddress = "25 rue Berri Montreal",
                                BuildingName = "Hôtel de Ville",
                             }
            });

            using (var context = new BookingDbContext(dbName))
            {
                var address = context.Find<PopularAddressDetails>(_popularAddressId);

                Assert.NotNull(address);
                Assert.AreEqual("25 rue Berri Montreal", address.FullAddress);
                Assert.AreEqual("Chez Costo2 popular !", address.FriendlyName);
                Assert.Null(address.RingCode);
                Assert.Null(address.Apartment);
                Assert.AreEqual("Hôtel de Ville", address.BuildingName);
                Assert.AreEqual(0, address.Latitude);
                Assert.AreEqual(0, address.Longitude);
            }
        }

        [Test]
        public void when_address_is_added_to_favorites_then_address_removed_from_history()
        {
            var addressId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var address = Guid.NewGuid().ToString();
            this.sut.Handle(new OrderCreated()
                                {
                                    AccountId = accountId,
                                    PickupAddress = new Address
                                    {
                                        FullAddress = address,
                                        Apartment = "3939",
                                        RingCode = "3131",
                                        BuildingName = "Hôtel de Ville",
                                        Latitude = 45.515065,
                                        Longitude = -73.558064,
                                    }
                                });
            this.sut.Handle(new FavoriteAddressAdded()
                                {
                                    SourceId = accountId,
                                    Address = new Address
               {
                   Id = addressId,
                   Apartment = "3939",
                   RingCode = "3131",
                   FriendlyName = "La Boite à Jojo",
                   BuildingName = "Hôtel de Ville",
                   FullAddress = address,
                   Longitude = -73.558064,
                   Latitude = 45.515065
               }

                                });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<AddressDetails>()
                    .Where(x => x.AccountId == accountId
                        && x.FullAddress.Equals(address)
                        && x.IsHistoric);
                Assert.AreEqual(0, list.Count());
            }
        }
    }

    [TestFixture]
    public class given_an_order : given_a_view_model_generator
    {
        [Test]
        public void when_order_created_pickup_then_address_is_added_to_history()
        {
            var orderId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var pickupDate = DateTime.Now.AddDays(1);
            var createdDate = DateTime.Now;
            this.sut.Handle(new OrderCreated
                                {
                                    SourceId = orderId,
                                    AccountId = accountId,
                                    PickupAddress = new Address()
                                                        {
                                                            FullAddress = "1234 rue Saint-Hubert",
                                                            Apartment = "3939",
                                                            RingCode = "3131",
                                                            BuildingName = "Hôtel de Ville",
                                                            Latitude = 45.515065,
                                                            Longitude = -73.558064,
                                                        },
                                   
                                    PickupDate = pickupDate,
                                    DropOffAddress =  new Address()
                                                          {
                                                              FriendlyName = "Velvet auberge st gabriel",
                                                              Latitude = 45.50643,
                                                              Longitude = -73.554052,
                                                          },
                                    CreatedDate = createdDate
                                });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<AddressDetails>().Where(x => x.AccountId == accountId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(accountId, dto.AccountId);
                Assert.AreEqual("3939", dto.Apartment);
                Assert.AreEqual("1234 rue Saint-Hubert", dto.FullAddress);
                Assert.AreEqual("3131", dto.RingCode);
                Assert.AreEqual("Hôtel de Ville", dto.BuildingName);
                Assert.AreEqual(45.515065, dto.Latitude);
                Assert.AreEqual(-73.558064, dto.Longitude);
            }
        }

        [Test]
        public void when_order_created_no_duplicate_address_is_added_to_history()
        {
            var command = new OrderCreated
                              {
                                  SourceId = Guid.NewGuid(),
                                  AccountId = Guid.NewGuid(),
                                  PickupAddress = new Address()
                                  {
                                      FullAddress = "1234 rue Saint-Hubert",
                                      Apartment = "3939",
                                      RingCode = "3131",
                                      Latitude = 45.515065,
                                      Longitude = -73.558064,
                                  },
                                  PickupDate = DateTime.Now,
                                  DropOffAddress = new Address()
                                  {
                                      FriendlyName = "Velvet auberge st gabriel",
                                      Latitude = 45.50643,
                                      Longitude = -73.554052,
                                  },
                                  CreatedDate = DateTime.Now.AddDays(-1)
                              };

            // Use the same address twice
            this.sut.Handle(command);
            this.sut.Handle(command);

            using (var context = new BookingDbContext(dbName))
            {
                var list =
                    context.Query<AddressDetails>().Where(
                        x => x.AccountId == command.AccountId && x.IsHistoric.Equals(true));
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(command.AccountId, dto.AccountId);
                Assert.AreEqual("3939", dto.Apartment);
                Assert.AreEqual("1234 rue Saint-Hubert", dto.FullAddress);
                //Assert.AreEqual("3131", dto.RingCode);
                Assert.AreEqual(45.515065, dto.Latitude);
                Assert.AreEqual(-73.558064, dto.Longitude);
            }
        }



        [Test]
        public void when_order_created_no_duplicate_address_is_added_to_history_even_if_optional_fields_are_not_set()
        {
            var command = new OrderCreated
                              {
                                  SourceId = Guid.NewGuid(),
                                  AccountId = Guid.NewGuid(),
                                  PickupAddress = new Address { FullAddress = "1234 rue Saint-Hubert" },
                                  CreatedDate = DateTime.Now.AddDays(-1)
                              };

            // Use the same address twice
            this.sut.Handle(command);
            this.sut.Handle(command);

            using (var context = new BookingDbContext(dbName))
            {
                var list =
                    context.Query<AddressDetails>().Where(
                        x => x.AccountId == command.AccountId && x.IsHistoric.Equals(true));
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(command.AccountId, dto.AccountId);
                Assert.AreEqual(null, dto.Apartment);
                Assert.AreEqual("1234 rue Saint-Hubert", dto.FullAddress);
                Assert.AreEqual(null, dto.RingCode);
                Assert.AreEqual(default(double), dto.Latitude);
                Assert.AreEqual(default(double), dto.Longitude);
            }
        }

        [Test]
        public void when_remove_address_not_more_in_db()
        {
            //Arrrange
            var orderCreated = new OrderCreated
            {
                SourceId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                PickupAddress = new Address { FullAddress = "1234 rue Saint-Hubert" },
                CreatedDate = DateTime.Now.AddDays(-1)
            };
            this.sut.Handle(orderCreated);

            Guid addressId;
            using (var context = new BookingDbContext(dbName))
            {
                addressId = context.Query<AddressDetails>().First(x => x.AccountId == orderCreated.AccountId && x.IsHistoric.Equals(true)).Id;
            }

            //Act
            this.sut.Handle(new AddressRemovedFromHistory() { AddressId = addressId });

            //Assert
            using (var context = new BookingDbContext(dbName))
            {
                var address = context.Query<AddressDetails>().FirstOrDefault(x => x.Id == addressId);
                Assert.IsNull(address);
            }
        }
    }
}


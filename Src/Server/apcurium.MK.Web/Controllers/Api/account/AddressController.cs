using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("account/addresses"), Auth]
    public class AddressController : BaseApiController
    {
        private readonly IAddressDao _addressDao;
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;


        private const int MaximumNumberEntriesInHistoryAddressList = 50;

        public AddressController(IAddressDao addressDao, IAccountDao accountDao, ICommandBus commandBus)
        {
            _addressDao = addressDao;
            _accountDao = accountDao;
            _commandBus = commandBus;
        }

        [HttpGet, Route]
        public IList<AddressDetails> GetAddresses()
        {
            return _addressDao.FindFavoritesByAccountId(GetSession().UserId);
        }

        [HttpGet, Route("history")]
        public Address[] GetAddressHistory()
        {
            var accountId = GetSession().UserId;

            var addresses = _addressDao.FindHistoricByAccountId(accountId);

            var addressDistinctComparer = new AddressDistinctComparer();

            return addresses
                .Select(address => new Address
                {
                    Id = address.Id,
                    FriendlyName = address.FriendlyName,
                    StreetNumber = address.StreetNumber,
                    Street = address.Street,
                    City = address.City,
                    ZipCode = address.ZipCode,
                    State = address.State,
                    FullAddress = address.FullAddress,
                    Longitude = address.Longitude,
                    Latitude = address.Latitude,
                    Apartment = address.Apartment,
                    RingCode = address.RingCode,
                    BuildingName = address.BuildingName,
                    IsHistoric = address.IsHistoric
                })
                .Distinct(addressDistinctComparer)
                .Take(MaximumNumberEntriesInHistoryAddressList)
                .ToArray();
        }

        [HttpDelete, Route("history/{addressId}")]
        public HttpResponseMessage Delete(Guid addressId)
        {
            var address = _addressDao.FindById(addressId);

            var account = _accountDao.FindById(GetSession().UserId);

            if (account.Id != address.AccountId)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Can't remove another account's address");
            }

            _commandBus.Send(new RemoveAddressFromHistory { AddressId = addressId, AccountId = account.Id });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        private class AddressDistinctComparer : IEqualityComparer<Address>
        {
            public bool Equals(Address x, Address y)
            {
                return x.DisplayLine1 == y.DisplayLine1 && x.DisplayLine2 == y.DisplayLine2;
            }

            public int GetHashCode(Address obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}

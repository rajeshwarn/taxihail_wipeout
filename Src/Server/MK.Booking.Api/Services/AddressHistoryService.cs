#region

using System;
using System.Net;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Web.Http;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class AddressHistoryService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        private readonly IAddressDao _dao;
		private readonly IOrderDao _orderDao;

		private static readonly int MaximumNumberEntriesInHistoryAddressList = 50;

		public AddressHistoryService(IAddressDao dao, ICommandBus commandBus, IAccountDao accountDao, IOrderDao orderDao)
        {
            _dao = dao;
            _commandBus = commandBus;
            _accountDao = accountDao;
			_orderDao = orderDao;
        }

        public object Get(AddressHistoryRequest request)
        {
			var accountId = new Guid(this.GetSession().UserAuthId);

			var addresses = _dao.FindHistoricByAccountId(accountId);

			var historyAddresses = from address in addresses
					select new Address()
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
					};

			var historyAddressesDistinct = from address in historyAddresses
										   group address by new { address.DisplayLine1, address.DisplayLine2 } into addressesDist
										   select addressesDist.First();

			return historyAddressesDistinct.Take(MaximumNumberEntriesInHistoryAddressList).ToArray();
        }

        public object Delete(AddressHistoryRequest request)
        {
            var address = _dao.FindById(request.AddressId);

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (account.Id != address.AccountId)
            {
                throw new HttpException(HttpStatusCode.Unauthorized, "Can't remove another account's address");
            }

            _commandBus.Send(new RemoveAddressFromHistory {AddressId = request.AddressId, AccountId = account.Id});

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
#region

using System;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.ServiceInterface;
using System.Collections;
using apcurium.MK.Booking.Domain;
using System.Collections.Generic;
using apcurium.MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class AccountOrderListService : BaseApiService
    {
        private readonly IOrderDao _orderDao;

		private readonly IOrderRatingsDao _orderRatingsDao;

		private readonly IServerSettings _serverSettings;

		public AccountOrderListService(IOrderDao orderDao, IOrderRatingsDao orderRatingsDao, IServerSettings serverSettings)
        {
			_orderDao = orderDao;
			_orderRatingsDao = orderRatingsDao;
			_serverSettings = serverSettings;
        }

        public AccountOrderListRequestResponse Get(AccountOrderListRequest request)
        {
            var orders = _orderDao.FindByAccountId(Session.UserId)
                .Where(x => !x.IsRemovedFromHistory)
                .OrderByDescending(c => c.CreatedDate)
                .Select(read => new OrderMapper().ToResource(read));

            var response = new AccountOrderListRequestResponse();
            response.AddRange(orders);
            return response;
        }

        public int Get(OrderCountForAppRatingRequest request)
		{
            // Count the number of orders for the account where the user left a minimum rating above the one defined in the settings (MinimumRideRatingScoreForAppRating)
            var ordersAboveThreshold = from allRatings in _orderRatingsDao.GetRatingsByAccountId(Session.UserId)
                                       group allRatings by allRatings.OrderId into ordersRatings
                                       select ordersRatings.Average(rt => rt.Score) into averageOrderRating
                                       where averageOrderRating >= _serverSettings.ServerData.MinimumRideRatingScoreForAppRating
                                       select averageOrderRating;

            return ordersAboveThreshold.Count();
		}
    }
}
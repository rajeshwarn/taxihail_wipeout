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
    public class AccountOrderListService : Service
    {
		protected readonly IOrderDao _orderDao;

		private readonly IOrderRatingsDao _orderRatingsDao;

		private readonly IServerSettings _serverSettings;

		public AccountOrderListService(IOrderDao orderDao, IOrderRatingsDao orderRatingsDao, IServerSettings serverSettings)
        {
			_orderDao = orderDao;
			_orderRatingsDao = orderRatingsDao;
			_serverSettings = serverSettings;
        }

        public object Get(AccountOrderListRequest request)
        {
            var session = this.GetSession();
            var orders = _orderDao.FindByAccountId(new Guid(session.UserAuthId))
                .Where(x => !x.IsRemovedFromHistory)
                .OrderByDescending(c => c.CreatedDate)
                .Select(read => new OrderMapper().ToResource(read));

            var response = new AccountOrderListRequestResponse();
            response.AddRange(orders);
            return response;
        }

		public object Get(AccountOrderListRequestWithUserIdRequest request)
		{
			var orderMapper = new OrderMapper();

			var orders = _orderDao.FindByAccountId(request.UserId)
				.Where(x => !x.IsRemovedFromHistory)
				.OrderByDescending(c => c.CreatedDate)
				.Select(read => orderMapper.ToResource(read));

			var response = new AccountOrderListRequestWithUserIdResponse();
			response.AddRange(orders);
			return response;
		}

		public object Get(AccountOrderNumberToAllowRatingRequest request)
		{
			var acccountId = new Guid(this.GetSession().UserAuthId);

			var successfulTripsForApplicationRating = from rating in _orderRatingsDao.GetRatingsByAccountId(acccountId)
					  group rating by rating.OrderId into ratingGrouped
					  select ratingGrouped.Average(rt => rt.Score) into averageScore
					  where averageScore >= _serverSettings.ServerData.MinimumRideRatingScoreForAppRating
					  select averageScore;

			return successfulTripsForApplicationRating.Count();
		}
    }
}
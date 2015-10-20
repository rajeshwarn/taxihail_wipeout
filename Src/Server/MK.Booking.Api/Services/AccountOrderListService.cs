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
		protected IOrderDao _orderDao { get; set; }
		
		private IOrderRatingsDao _orderRatingsDao { get; set; }

		private IServerSettings _serverSettings;

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
			var userId = new Guid(this.GetSession().UserAuthId);

			var successfulTripsForApplicationRATe = from order in _orderDao.FindByAccountId(userId)
					  from rating in _orderRatingsDao.GetRatings()
					  where rating.OrderId == order.Id && order.IsRated && order.Status == (int)apcurium.MK.Common.Entity.OrderStatus.Completed
					  group rating by rating.OrderId into ratingGrouped
					  select ratingGrouped.Average(rt => rt.Score) into averageScore
					  where averageScore >= _serverSettings.ServerData.RateMobileMinimumRideRatingForSuccessfulTrip
					  select averageScore;

			return successfulTripsForApplicationRATe.Count();
		}
    }
}
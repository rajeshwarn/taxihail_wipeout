using System.Linq;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("account")]
    public class AccountOrderController : BaseApiController
    {
        private readonly IOrderDao _orderDao;

        private readonly IOrderRatingsDao _orderRatingsDao;

        private readonly IServerSettings _serverSettings;

        public AccountOrderController(IOrderDao orderDao, IOrderRatingsDao orderRatingsDao, IServerSettings serverSettings)
        {
            _orderDao = orderDao;
            _orderRatingsDao = orderRatingsDao;
            _serverSettings = serverSettings;
        }

        [HttpGet]
        [Route("ordercountforapprating")]
        public int GetOrderCountForAppRating()
        {
            var accountId = GetSession().UserId;

            // Count the number of orders for the account where the user left a minimum rating above the one defined in the settings (MinimumRideRatingScoreForAppRating)
            var ordersAboveThreshold = from allRatings in _orderRatingsDao.GetRatingsByAccountId(accountId)
                                       group allRatings by allRatings.OrderId into ordersRatings
                                       select ordersRatings.Average(rt => rt.Score) into averageOrderRating
                                       where averageOrderRating >= _serverSettings.ServerData.MinimumRideRatingScoreForAppRating
                                       select averageOrderRating;

            return ordersAboveThreshold.Count();
        }

        [HttpGet, NoCache]
        [Route("orders")]
        public AccountOrderListRequestResponse GetOrderListForAccount()
        {
            var session = GetSession();
            var orders = _orderDao.FindByAccountId(session.UserId)
                .Where(x => !x.IsRemovedFromHistory)
                .OrderByDescending(c => c.CreatedDate)
                .Select(read => new OrderMapper().ToResource(read));

            var response = new AccountOrderListRequestResponse();
            response.AddRange(orders);
            return response;
        }

    }
}

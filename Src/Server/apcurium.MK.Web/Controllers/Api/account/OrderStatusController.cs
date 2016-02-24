using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Web.Security;
using AutoMapper;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/account/orders")]
    [NoCache]
    [Auth]
    public class OrderStatusController : BaseApiController
    {
        private readonly OrderStatusHelper _orderStatusHelper;
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;

        public OrderStatusController(OrderStatusHelper orderStatusHelper, IAccountDao accountDao, IOrderDao orderDao)
        {
            _orderStatusHelper = orderStatusHelper;
            _accountDao = accountDao;
            _orderDao = orderDao;
        }

        [HttpGet]
        [Route("{OrderId}/status/")]
        public async Task<OrderStatusRequestResponse> GetOrderStatus(Guid orderId)
        {
            var session = GetSession();

            var status = await _orderStatusHelper.GetOrderStatus(orderId, session);

            return Mapper.Map<OrderStatusRequestResponse>(status);
        }

        [HttpGet]
        [Route("status/active")]
        public ActiveOrderStatusRequestResponse GetActiveOrdersStatus()
        {
            var statuses = new ActiveOrderStatusRequestResponse();
            var account = _accountDao.FindById(GetSession().UserId);

            foreach (var status in _orderDao.GetOrdersInProgressByAccountId(account.Id))
            {
                statuses.Add(status);
            }

            return statuses;
        }
    }
}

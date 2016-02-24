#region

using System;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;

#endregion

namespace apcurium.MK.Booking.Api.Services
{

    [RoutePrefix("api")]
    public class OrderStatusService : Services
    {
        
    }

    public class ActiveOrderStatusService : BaseApiController
    {
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;

        public ActiveOrderStatusService(IOrderDao orderDao, IAccountDao accountDao)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
        }

        public object Get(ActiveOrderStatusRequest request)
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
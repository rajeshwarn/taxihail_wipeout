#region

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class CancelOrderService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly ICommandBus _commandBus;
        private readonly IOrderDao _orderDao;
        private readonly IUpdateOrderStatusJob _updateOrderStatusJob;

        public CancelOrderService(ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider,
            IOrderDao orderDao, IAccountDao accountDao, IUpdateOrderStatusJob updateOrderStatusJob)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _orderDao = orderDao;
            _accountDao = accountDao;
            _updateOrderStatusJob = updateOrderStatusJob;
            _commandBus = commandBus;
        }

        public object Post(CancelOrder request)
        {
            var order = _orderDao.FindById(request.OrderId);
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (order == null)
            {
                return new HttpResult(HttpStatusCode.NotFound);
            }

            if (account.Id != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't cancel another account's order");
            }

            if (order.IBSOrderId.HasValue)
            {
                var currentIbsAccountId = _accountDao.GetIbsAccountId(account.Id, order.CompanyKey);
                if (currentIbsAccountId.HasValue)
                {
                    //We need to try many times because sometime the IBS cancel method doesn't return an error but doesn't cancel the ride... after 5 time, we are giving up. But we assume the order is completed.
                    Task.Factory.StartNew(() =>
                    {
                        Func<bool> cancelOrder = () => _ibsServiceProvider.Booking(order.CompanyKey).CancelOrder(order.IBSOrderId.Value, currentIbsAccountId.Value, order.Settings.Phone);
                        cancelOrder.Retry(new TimeSpan(0, 0, 0, 10), 5);
                    });
                }
            }
            
            var command = new Commands.CancelOrder { OrderId = request.OrderId };
            _commandBus.Send(command);

            UpdateStatusAsync(command.Id);

            return new HttpResult(HttpStatusCode.OK);
        }

        private void UpdateStatusAsync(Guid id)
        {
            new TaskFactory().StartNew(() =>
            {
                //We have to wait for the order to be completed.
                Thread.Sleep(750);
                _updateOrderStatusJob.CheckStatus(id);
            });
        }
    }
}
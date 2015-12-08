using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderUpdateService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;

        public OrderUpdateService(IOrderDao dao, IAccountDao accountDao, ICommandBus commandBus, IBookingWebServiceClient bookingWebServiceClient)
        {
            Dao = dao;
            _accountDao = accountDao;
            _commandBus = commandBus;
            _bookingWebServiceClient = bookingWebServiceClient;
        }

        protected IOrderDao Dao { get; set; }

        public object Post(OrderUpdateRequest request)
        {
            var order = Dao.FindById(request.OrderId);
            if (order == null || !order.IBSOrderId.HasValue)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.OrderNotInIbs.ToString());
            }

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (account.Id != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't access another account's order");
            }

            // If the order was created in another company, need to fetch the correct IBS account
            var ibsAccountId = _accountDao.GetIbsAccountId(account.Id, order.CompanyKey);

            if (!ibsAccountId.HasValue)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.IBSAccountNotFound.ToString());
            }

            var success = _bookingWebServiceClient.UpdateDropOffInTrip(order.IBSOrderId.Value, ibsAccountId.Value, request.DropOffAddress);

            if (success)
            {
                _commandBus.Send(new UpdateOrderInTrip { OrderId = request.OrderId, DropOffAddress = request.DropOffAddress });
            }

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}

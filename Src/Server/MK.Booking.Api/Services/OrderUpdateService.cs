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
        private readonly IIBSServiceProvider _ibsServiceProvider;

        public OrderUpdateService(IOrderDao dao, IAccountDao accountDao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider)
        {
            Dao = dao;
            _accountDao = accountDao;
            _commandBus = commandBus;
            _ibsServiceProvider = ibsServiceProvider;
        }

        protected IOrderDao Dao { get; set; }

        public object Post(OrderUpdateRequest request)
        {
            var order = Dao.FindById(request.OrderId);
            if (order == null || !order.IBSOrderId.HasValue)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.OrderNotInIbs.ToString());
            }

            var success = _ibsServiceProvider.Booking().UpdateDropOffInTrip(order.IBSOrderId.Value, order.Id, request.DropOffAddress);
            
            if (success)
            {
                _commandBus.Send(new UpdateOrderInTrip { OrderId = request.OrderId, DropOffAddress = request.DropOffAddress });
            }

            return success;
        }
    }
}

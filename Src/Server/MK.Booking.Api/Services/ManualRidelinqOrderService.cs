using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class ManualRidelinqOrderService : Service
    {
        private readonly IOrderDao _orderDao;
        private readonly ICommandBus _commandBus;

        public ManualRidelinqOrderService(ICommandBus commandBus, IOrderDao orderDao)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
        }

        public object Post(ManualRideLinqPairingRequest rideLinqRequest)
        {
            var accountId = new Guid(this.GetSession().UserAuthId);

            var command = new CreateOrderForManualRideLinqPair
            {
                AccountId = accountId,
                UserAgent = Request.UserAgent,
                ClientVersion = Request.Headers.Get("ClientVersion"),
                PairingCode = rideLinqRequest.PairingCode,
                Longitude = rideLinqRequest.Longitude,
                Latitude = rideLinqRequest.Latitude,
                ClientLanguageCode = rideLinqRequest.ClientLanguageCode
            };

            _commandBus.Send(command);

            return new OrderStatusDetail
            {
                OrderId = command.Id,
                Status = OrderStatus.Created,
                IBSStatusId = string.Empty,
                IBSStatusDescription = string.Empty,
                IsManualRideLinq = true
            };
        }

        public object Get(Guid orderId)
        {
            var item = _orderDao.GetManualRideLinqById(orderId);

            return new OrderMapper().ToResource(item);
        }

        public object Delete(Guid orderId)
        {
            _commandBus.Send(new UnpairOrderForManualRideLinq {OrderId = orderId});

            return new HttpResult(HttpStatusCode.OK); ;
        }
    }
}

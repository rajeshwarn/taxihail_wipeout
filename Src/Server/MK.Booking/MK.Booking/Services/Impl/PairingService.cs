using System;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Services.Impl
{
    public class PairingService : IPairingService
    {
        private readonly ICommandBus _commandBus;
        private readonly IIbsOrderService _ibs;
        private readonly IOrderDao _orderDao;
        private readonly Resources.Resources _resources;

        public PairingService(ICommandBus commandBus, IIbsOrderService ibs, IOrderDao orderDao, IServerSettings serverSettings)
        {
            _commandBus = commandBus;
            _ibs = ibs;
            _orderDao = orderDao;

            _resources = new Resources.Resources(serverSettings);
        }

        public void Pair(Guid orderId, string cardToken, int autoTipPercentage)
        {
            var orderStatusDetail = _orderDao.FindOrderStatusById(orderId);
            if (orderStatusDetail == null)
            {
                throw new Exception("Order not found");
            }

            if (orderStatusDetail.IBSOrderId == null)
            {
                throw new Exception("Order has no IBSOrderId");
            }

            var orderPairingDetail = _orderDao.FindOrderPairingById(orderId);
            if (orderPairingDetail != null)
            {
                // Do not throw an exception as the client would interpret this as an "error"
                return;
            }
            
            // send a command to save the pairing state for this order
            _commandBus.Send(new PairForPayment
            {
                OrderId = orderId,
                TokenOfCardToBeUsedForPayment = cardToken,
                AutoTipPercentage = autoTipPercentage
            });
        }

        public void Unpair(Guid orderId)
        {
            var orderPairingDetail = _orderDao.FindOrderPairingById(orderId);
            if (orderPairingDetail == null)
            {
                throw new Exception("Order Pairing not found");
            }

            var orderStatusDetail = _orderDao.FindOrderStatusById(orderId);
            if (orderStatusDetail == null)
            {
                throw new Exception("Order not found");
            }

            // send a message to driver, if it fails we abort the unpairing
			_ibs.SendMessageToDriver(_resources.Get("UnpairingConfirmationToDriver"), orderStatusDetail.VehicleNumber, orderStatusDetail.CompanyKey);

            // send a command to delete the pairing pairing info for this order
            _commandBus.Send(new UnpairForPayment
            {
                OrderId = orderId
            });
        }
    }
}
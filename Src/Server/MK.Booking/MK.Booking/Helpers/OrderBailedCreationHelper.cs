using System;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Helpers
{
    public class OrderBailedCreationHelper
    {
        private readonly IOrderDao _orderDao;
        private readonly ICommandBus _commandBus;
        private readonly ILogger _logger;
        private readonly Resources.Resources _resources;

        public OrderBailedCreationHelper(IOrderDao orderDao, ICommandBus commandBus, IServerSettings serverSettings, ILogger logger)
        {
            _orderDao = orderDao;
            _commandBus = commandBus;
            _logger = logger;
            _resources = new Resources.Resources(serverSettings);
        }

        public bool SendOrderCreationCommands(Guid orderId, int? ibsOrderId, bool dispatcherTimedOut, string clientLanguageCode)
        {
            if (!ibsOrderId.HasValue
                || ibsOrderId <= 0)
            {
                var code = !ibsOrderId.HasValue || (ibsOrderId.Value >= -1) ? (int?)null : Math.Abs(ibsOrderId.Value);
                var errorCode = "CreateOrder_CannotCreateInIbs_" + code;

                var errorCommand = new CancelOrderBecauseOfError
                {
                    OrderId = orderId,
                    ErrorCode = errorCode
                };

                if (dispatcherTimedOut)
                {
                    errorCommand.DispatcherTimedOut = true;
                    errorCommand.ErrorDescription = _resources.Get("OrderStatus_wosTIMEOUT", clientLanguageCode);
                }
                else
                {
                    errorCommand.ErrorDescription = _resources.Get(errorCode, clientLanguageCode);
                }

                _commandBus.Send(errorCommand);

                return false;
            }
            else
            {
                _logger.LogMessage(string.Format("Adding IBSOrderId {0} to order {1}", ibsOrderId, orderId));

                var ibsCommand = new AddIbsOrderInfoToOrder
                {
                    OrderId = orderId,
                    IBSOrderId = ibsOrderId.Value
                };
                _commandBus.Send(ibsCommand);

                return true;
            }
        }
    }
}

using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Diagnostic;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderCancellationManager : IIntegrationEventHandler,
        IEventHandler<IbsOrderInfoAddedToOrder_V2>
    {
        private readonly IOrderDao _orderDao;
        private readonly IIbsCreateOrderService _ibsCreateOrderService;
        private readonly ILogger _logger;

        public OrderCancellationManager(IOrderDao orderDao,
            IIbsCreateOrderService ibsCreateOrderService,
            ILogger logger)
        {
            _orderDao = orderDao;
            _ibsCreateOrderService = ibsCreateOrderService;
            _logger = logger;
        }

        public void Handle(IbsOrderInfoAddedToOrder_V2 @event)
        {
            if (!@event.CancelWasRequested)
            {
                return;
            }

            // User decided to cancel his order before we got assigned an ibs order id
            // Since we now have the ibs order id and the order is already flagged as cancelled on our side, we have to cancel it on ibs
            var orderDetail = _orderDao.FindById(@event.SourceId);

            _logger.LogMessage(
                string.Format("User requested a cancellation before IbsOrderId was received, cancel the order on ibs now that we have it (OrderId: {0}, IbsOrderId: {1}, CompanyKey {2})",
                    @event.SourceId,
                    @event.IBSOrderId,
                    @event.CompanyKey
                    ));

            // Cancel order on current company IBS
            _ibsCreateOrderService.CancelIbsOrder(@event.IBSOrderId, @event.CompanyKey, orderDetail.Settings.Phone, orderDetail.AccountId);
        }
    }
}
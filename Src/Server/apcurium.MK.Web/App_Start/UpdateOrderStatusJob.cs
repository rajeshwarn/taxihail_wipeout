using System;
using System.Web;
using System.Web.Caching;
using Infrastructure.Messaging;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using log4net;

namespace apcurium.MK.Web.App_Start
{
    public class UpdateOrderStatusJob
    {
        private readonly IOrderDao _orderDao;
        private readonly IConfigurationManager _configManager;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly ICommandBus _commandBus;
        private const string CacheKey = "OrderStatusJob";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UpdateOrderStatusJob));

        public UpdateOrderStatusJob(IOrderDao orderDao, IConfigurationManager configManager, IBookingWebServiceClient bookingWebServiceClient, ICommandBus commandBus)
        {
            _orderDao = orderDao;
            _configManager = configManager;
            _bookingWebServiceClient = bookingWebServiceClient;
            _commandBus = commandBus;
        }

        public void Start()
        {
            if (HttpRuntime.Cache[CacheKey] == null)
            {
                Logger.Debug("Add OrderStatusJob in Cache");
                HttpRuntime.Cache.Insert(CacheKey, new object(), null, DateTime.Now.AddSeconds(10), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, CacheItemRemoved);
            }
        }

        private void CacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            Logger.Debug("OrderStatus Job Start New Instance");
            try
            {
                var orders = _orderDao.GetOrdersInProgress();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
            }
            finally
            {
                HttpRuntime.Cache.Insert(CacheKey, new object(), null, DateTime.Now.AddSeconds(10), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, CacheItemRemoved);
            }
        }
    }
}
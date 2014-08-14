#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using AutoMapper;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class OrderDao : IOrderDao
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly Resources.Resources _resources;

        private const int TaxiDistanceThresholdForPushNotification = 200; // In meters

        public OrderDao(Func<BookingDbContext> contextFactory, IPushNotificationService pushNotificationService, IConfigurationManager configManager)
        {
            _contextFactory = contextFactory;
            _pushNotificationService = pushNotificationService;

            _resources = new Resources.Resources(configManager.GetSetting("TaxiHail.ApplicationKey"));
        }

        public IList<OrderDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderDetail>().ToList();
            }
        }

        public OrderDetail FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderDetail>().SingleOrDefault(c => c.Id == id);
            }
        }

        public IList<OrderDetail> FindByAccountId(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderDetail>().Where(c => c.AccountId == id).ToList();
            }
        }

        public IList<OrderDetailWithAccount> GetAllWithAccountSummary()
        {
            var list = new List<OrderDetailWithAccount>();
            using (var context = _contextFactory.Invoke())
            {
                var joinedLines = from order in context.Set<OrderDetail>()
                    join account in context.Set<AccountDetail>()
                        on order.AccountId equals account.Id
                    join payment in context.Set<OrderPaymentDetail>()
                        on order.Id equals payment.OrderId into orderPayment
                    from payment in orderPayment.DefaultIfEmpty()
                    join status in context.Set<OrderStatusDetail>()
                        on order.Id equals status.OrderId into statusOrder
                    from status in statusOrder.DefaultIfEmpty()
                    join rating in context.Set<RatingScoreDetails>()
                        on order.Id equals rating.OrderId into ratingOrder
                    from rating in ratingOrder.DefaultIfEmpty()
                    select new {order, account, payment, status, rating};

                OrderDetailWithAccount details = null;

                foreach (var joinedLine in joinedLines)
                {
                    if (details == null || details.IBSOrderId != joinedLine.order.IBSOrderId)
                    {
                        if (details != null)
                            list.Add(details);
                        details = new OrderDetailWithAccount();
                        Mapper.Map(joinedLine.account, details);
                        Mapper.Map(joinedLine.order, details);
                        if (joinedLine.payment != null)
                        {
                            Mapper.Map(joinedLine.payment, details);
                        }

                        if (joinedLine.status != null)
                        {
                            Mapper.Map(joinedLine.status, details);
                        }
                    }

                    if (joinedLine.rating != null)
                        details.Rating[joinedLine.rating.Name] =
                            joinedLine.rating.Score.ToString(CultureInfo.InvariantCulture);
                }
                list.Add(details);
            }
            return list;
        }

        public IList<OrderStatusDetail> GetOrdersInProgress()
        {
            using (var context = _contextFactory.Invoke())
            {
                
                var startDate = DateTime.Now.AddHours(-36);

                var currentOrders = (from order in context.Set<OrderStatusDetail>()
                                     where (order.Status == OrderStatus.Created
                                        || order.Status == OrderStatus.Pending) && (order.PickupDate >= startDate)
                                     select order).ToList();
                return currentOrders;
            }
        }

        public IList<OrderStatusDetail> GetOrdersInProgressByAccountId(Guid accountId)
        {
            using (var context = _contextFactory.Invoke())
            {
                var startDate = DateTime.Now.AddHours(-36);

                var currentOrders = (from order in context.Set<OrderStatusDetail>()
                    where order.AccountId == accountId
                    where (order.Status == OrderStatus.Created
                        || order.Status == OrderStatus.Pending)
                        && (order.PickupDate >= startDate)
                    select order).ToList();
                return currentOrders;
            }
        }

        public OrderStatusDetail FindOrderStatusById(Guid orderId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderStatusDetail>().SingleOrDefault(x => x.OrderId == orderId);
            }
        }

        public OrderPairingDetail FindOrderPairingById(Guid orderId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderPairingDetail>().SingleOrDefault(x => x.OrderId == orderId);
            }
        }

        public void UpdateVehiclePosition(Guid orderId, string ibsStatus, double? newLatitude, double? newLongitude, out bool taxiNearbyPushSent)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Query<OrderDetail>().Single(x => x.Id == orderId);
                var orderStatus = context.Query<OrderStatusDetail>().Single(x => x.OrderId == orderId);

                taxiNearbyPushSent = orderStatus.IsTaxiNearbyNotificationSent;
                var shouldSendPushNotification = newLatitude.HasValue &&
                                                 newLongitude.HasValue &&
                                                 ibsStatus == VehicleStatuses.Common.Assigned &&
                                                 !taxiNearbyPushSent;

                // update vehicle position in orderStatus
                orderStatus.VehicleLatitude = newLatitude;
                orderStatus.VehicleLongitude = newLongitude;
                context.Save(orderStatus);

                if (shouldSendPushNotification)
                {
                    var taxiPosition = new Position(newLatitude.Value, newLongitude.Value);
                    var pickupPosition = new Position(order.PickupAddress.Latitude, order.PickupAddress.Longitude);

                    if (taxiPosition.DistanceTo(pickupPosition) <= TaxiDistanceThresholdForPushNotification)
                    {
                        orderStatus.IsTaxiNearbyNotificationSent = true;
                        taxiNearbyPushSent = true;
                        context.Save(orderStatus);

                        var alert = string.Format(_resources.Get("PushNotification_NearbyTaxi", order.ClientLanguageCode));
                        var data = new Dictionary<string, object> { { "orderId", order.Id } };
                        var devices = context.Set<DeviceDetail>().Where(x => x.AccountId == order.AccountId);

                        // Send push notifications
                        foreach (var device in devices)
                        {
                            _pushNotificationService.Send(alert, data, device.DeviceToken, device.Platform);
                        }
                    }
                }
            }
        }
    }
}
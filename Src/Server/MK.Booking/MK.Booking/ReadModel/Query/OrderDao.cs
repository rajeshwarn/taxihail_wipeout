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
                                  join card in context.Set<CreditCardDetails>()
                                      on account.DefaultCreditCard equals card.CreditCardId into accountCard
                                  from card in accountCard.DefaultIfEmpty()
                                  join payment in context.Set<OrderPaymentDetail>()
                                      on order.Id equals payment.OrderId into orderPayment
                                  from payment in orderPayment.DefaultIfEmpty()
                                  join status in context.Set<OrderStatusDetail>()
                                      on order.Id equals status.OrderId into statusOrder
                                  from status in statusOrder.DefaultIfEmpty()
                                  join rating in context.Set<RatingScoreDetails>()
                                      on order.Id equals rating.OrderId into ratingOrder
                                  from rating in ratingOrder.DefaultIfEmpty()
                                  select new { order, account, payment, status, rating, card };

                OrderDetailWithAccount details = null;

                foreach (var joinedLine in joinedLines)
                {
                    if (details == null || details.IBSOrderId != joinedLine.order.IBSOrderId)
                    {
                        if (details != null)
                        {
                            list.Add(details);
                        }

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

                        if (joinedLine.status != null)
                        {
                            Mapper.Map(joinedLine.card, details);
                        }
                    }

                    details.IsCompleted = joinedLine.status.Status == OrderStatus.Completed;
                    details.IsCancelled = joinedLine.status.Status == OrderStatus.Canceled;

                    if (joinedLine.rating != null)
                    {
                        details.Rating[joinedLine.rating.Name] =
                            joinedLine.rating.Score.ToString(CultureInfo.InvariantCulture);
                    }      
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
                                        || order.Status == OrderStatus.Pending
                                        || order.Status == OrderStatus.WaitingForPayment) && (order.PickupDate >= startDate)
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

        public void UpdateVehiclePosition(Guid orderId, double? newLatitude, double? newLongitude)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderStatus = context.Query<OrderStatusDetail>().Single(x => x.OrderId == orderId);

                orderStatus.VehicleLatitude = newLatitude;
                orderStatus.VehicleLongitude = newLongitude;

                context.Save(orderStatus);
            }
        }
    }
}
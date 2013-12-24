using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Common.Entity;
using AutoMapper;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class OrderDao : IOrderDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public OrderDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<OrderDetail> GetAll()
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                return context.Query<OrderDetail>().ToList();
            }
        }

        public OrderDetail FindById(Guid id)
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                return context.Query<OrderDetail>().SingleOrDefault(c => c.Id == id);
            }
        }

        public IList<OrderDetail> FindByAccountId(Guid id)
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                return context.Query<OrderDetail>().Where(c => c.AccountId == id).ToList();
            }
        }

        public IList<OrderDetailWithAccount> GetAllWithAccountSummary()
        {
            var list = new List<OrderDetailWithAccount>();
            using (BookingDbContext context = _contextFactory.Invoke())
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
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                List<OrderStatusDetail> currentOrders = (from order in context.Set<OrderStatusDetail>()
                    where order.Status != OrderStatus.Canceled
                          && order.Status != OrderStatus.Completed
                          && order.Status != OrderStatus.TimedOut
                    select order).ToList();
                return currentOrders;
            }
        }

        public IList<OrderStatusDetail> GetOrdersInProgressByAccountId(Guid accountId)
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                List<OrderStatusDetail> currentOrders = (from order in context.Set<OrderStatusDetail>()
                    where order.AccountId == accountId
                    where order.Status != OrderStatus.Canceled && order.Status != OrderStatus.Completed
                    select order).ToList();
                return currentOrders;
            }
        }

        public OrderStatusDetail FindOrderStatusById(Guid orderId)
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                return context.Query<OrderStatusDetail>().SingleOrDefault(x => x.OrderId == orderId);
            }
        }

        public OrderPairingDetail FindOrderPairingById(Guid orderId)
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                return context.Query<OrderPairingDetail>().SingleOrDefault(x => x.OrderId == orderId);
            }
        }
    }
}
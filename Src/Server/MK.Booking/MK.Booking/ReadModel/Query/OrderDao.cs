using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Common.Entity;

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
                                  select new {order,account} ;

                foreach (var joinedLine in joinedLines)
                {
                    var details = new OrderDetailWithAccount();
                    AutoMapper.Mapper.Map(joinedLine.account, details);
                    AutoMapper.Mapper.Map(joinedLine.order, details);
                    list.Add(details);
                }
            }
            return list;
        }

        public IList<OrderStatusDetail> GetOrdersInProgress()
        {
            using (var context = _contextFactory.Invoke())
            {
                var currentOrders = (from order in context.Set<OrderStatusDetail>()
                                     where order.Status != OrderStatus.Canceled 
                                        && order.Status != OrderStatus.Completed
                                        && order.Status != OrderStatus.TimedOut
                                     select order).ToList();
                return currentOrders;
            }
        }

        public IList<OrderStatusDetail> GetOrdersInProgressByAccountId(Guid accountId)
        {
            using (var context = _contextFactory.Invoke())
            {
                var currentOrders = (from order in context.Set<OrderStatusDetail>()
                                     where order.AccountId == accountId
                                     where order.Status != Common.Entity.OrderStatus.Canceled && order.Status != Common.Entity.OrderStatus.Completed
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

    }
}

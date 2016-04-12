using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Web.Areas.AdminTH.Models;
using apcurium.MK.Web.Attributes;
using Infrastructure.Sql.EventSourcing;
using Newtonsoft.Json;
using ServiceStack.CacheAccess;
using ServiceStack.Common;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    [AuthorizationRequired(RoleName.Admin)]
    public class OrderController : ServiceStackController
    {
        private readonly Func<EventStoreDbContext> _eventsContextFactory;
        private readonly Func<BookingDbContext> _bookingContextFactory;

        public OrderController(ICacheClient cache, IServerSettings serverSettings, Func<EventStoreDbContext> eventsContextFactory, Func<BookingDbContext> bookingContextFactory)
            : base(cache, serverSettings)
        {
            _eventsContextFactory = eventsContextFactory;
            _bookingContextFactory = bookingContextFactory;
        }

        // GET: AdminTH/Order/ViewDebug/{id}
        public ActionResult ViewDebug(string id)
        {
            var ibsId = id.ToInt(int.MinValue);
            Guid orderId;
            var isOrderId = Guid.TryParse(id, out orderId);

            OrderDebugModel model;

            if (ibsId != int.MinValue)
            {
                model = GenerateOrderDebugModel(ibsId);
            }
            else if (isOrderId)
            {
                model = GenerateOrderDebugModel(orderId);
            }
            else
            {
                model = new OrderDebugModel();
            }

            return PartialView(model);
        }

        private OrderDebugModel GenerateOrderDebugModel(Guid orderId)
        {
            OrderDebugModel model;
            var orderAndPaymentIds = new List<Guid>();
            using (var context = _bookingContextFactory.Invoke())
            {
                var orderDetails = context.Query<OrderDetail>().FirstOrDefault(x => x.Id == orderId);
                if (orderDetails == null)
                {
                    return new OrderDebugModel();
                }

                model = UpdateModelWithBookingContext(orderDetails, context);

                orderAndPaymentIds.Add(orderId);
                if (model.OrderPaymentDetail != null)
                {
                    orderAndPaymentIds.Add(model.OrderPaymentDetail.PaymentId);
                }
            }

            model.RelatedEvents = GetEvents(orderAndPaymentIds);

            return model;
        }
        
        private OrderDebugModel GenerateOrderDebugModel(int id)
        {
            OrderDebugModel model;

            var orderAndPaymentIds = new List<Guid>();
            using (var context = _bookingContextFactory.Invoke())
            {
                var orderDetails = context.Query<OrderDetail>().FirstOrDefault(x => x.IBSOrderId == id);
                if (orderDetails == null)
                {
                    return new OrderDebugModel();
                }

                model = UpdateModelWithBookingContext(orderDetails, context);

                orderAndPaymentIds.Add(model.OrderDetail.Id);
                if (model.OrderPaymentDetail != null)
                {
                    orderAndPaymentIds.Add(model.OrderPaymentDetail.PaymentId);
                }
            }

            model.RelatedEvents = GetEvents(orderAndPaymentIds);

            return model;
        }

        private static OrderDebugModel UpdateModelWithBookingContext(OrderDetail orderDetail, BookingDbContext context)
        {
            return new OrderDebugModel()
            {
                OrderDetail = orderDetail,
                UserEmail = context.Find<AccountDetail>(orderDetail.AccountId).Email,
                OrderStatusDetail = context.Find<OrderStatusDetail>(orderDetail.Id),
                OrderPairingDetail = context.Find<OrderPairingDetail>(orderDetail.Id),
                OrderPaymentDetail = context.Query<OrderPaymentDetail>().FirstOrDefault(x => x.OrderId == orderDetail.Id),
                OverduePaymentDetail = context.Query<OverduePaymentDetail>().FirstOrDefault(x => x.OrderId == orderDetail.Id)
            };
        }

        private string GetEvents(List<Guid> orderAndPaymentIds)
        {
            using (var context = _eventsContextFactory.Invoke())
            {
                var events = context.Set<Event>()
                        .Where(x => orderAndPaymentIds.Contains(x.AggregateId))
                        .OrderBy(x => x.EventDate)
                        .ThenBy(x => x.Version)
                        .ToList() // Cast needed  because LinqToSql doesn't support JsonConvert
                        .Select(x => new
                        {
                            x.EventDate,
                            x.EventType,
                            Payload = JsonConvert.DeserializeObject(x.Payload)
                        });


                return JsonConvert.SerializeObject(events);
            }
        }
    }
}
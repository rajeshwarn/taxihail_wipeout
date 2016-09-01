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

        // GET: AdminTH/Order/ViewDebug/5
        public ActionResult ViewDebug(int id)
        {
            var model = new OrderDebugModel();

            var orderAndPaymentIds = new List<Guid>();
            using (var context = _bookingContextFactory.Invoke())
            {
                model.OrderDetail = context.Query<OrderDetail>().FirstOrDefault(x => x.IBSOrderId == id);
                if (model.OrderDetail == null)
                {
                    return PartialView(model);
                }

                model.UserEmail = context.Find<AccountDetail>(model.OrderDetail.AccountId).Email;
                model.OrderStatusDetail = context.Find<OrderStatusDetail>(model.OrderDetail.Id);
                model.OrderPairingDetail = context.Find<OrderPairingDetail>(model.OrderDetail.Id);
                model.OrderPaymentDetail = context.Query<OrderPaymentDetail>().FirstOrDefault(x => x.OrderId == model.OrderDetail.Id);
                model.OverduePaymentDetail = context.Query<OverduePaymentDetail>().FirstOrDefault(x => x.OrderId == model.OrderDetail.Id);

                orderAndPaymentIds.Add(model.OrderDetail.Id);
                if (model.OrderPaymentDetail != null)
                {
                    orderAndPaymentIds.Add(model.OrderPaymentDetail.PaymentId);
                }
            }

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
                model.RelatedEvents = JsonConvert.SerializeObject(events);
            }

            return PartialView(model);
        }
    }
}
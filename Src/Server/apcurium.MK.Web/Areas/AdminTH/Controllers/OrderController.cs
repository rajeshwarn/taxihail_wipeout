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

        // GET: AdminTH/Order/{id}
        public ActionResult ViewDebug(Guid id)
        {
            var model = new OrderDebug();
            var relevantIds = new List<Guid> {id};

            using (var context = _bookingContextFactory.Invoke())
            {
                var orderDetail = context.Find<OrderDetail>(id);
                model.OrderDetail = orderDetail;
                if (orderDetail == null)
                {
                    return View(model);
                }

                model.UserEmail = context.Find<AccountDetail>(orderDetail.AccountId).Email;
                model.OrderStatusDetail = context.Find<OrderStatusDetail>(id);
                model.OrderPairingDetail = context.Find<OrderPairingDetail>(id);
                model.OrderPaymentDetail = context.Query<OrderPaymentDetail>().FirstOrDefault(x => x.OrderId == id);

                if (model.OrderPaymentDetail != null)
                {
                    relevantIds.Add(model.OrderPaymentDetail.PaymentId);
                }
            }

            using (var context = _eventsContextFactory.Invoke())
            {
                var events = context.Set<Event>()
                        .Where(x => relevantIds.Contains(x.AggregateId))
                        .OrderBy(x => x.EventDate)
                        .ThenBy(x => x.Version)
                        .ToList();
                model.RelatedEvents = events;
            }

            return View(model);
        }
    }
}
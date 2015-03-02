using System.Collections.Generic;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;
using Infrastructure.Sql.EventSourcing;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
    public class OrderDebug
    {
        public OrderDebug()
        {
            RelatedEvents = new List<Event>();
        }

        public List<Event> RelatedEvents { get; set; }

        public OrderDetail OrderDetail { get; set; }
        public OrderStatusDetail OrderStatusDetail { get; set; }
        public OrderPairingDetail OrderPairingDetail { get; set; }
        public OrderPaymentDetail OrderPaymentDetail { get; set; }

        public string UserEmail { get; set; }
    }
}
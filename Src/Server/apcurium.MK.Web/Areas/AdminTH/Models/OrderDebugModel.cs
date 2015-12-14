using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
	public class OrderDebugModel
	{
		public string UserEmail { get; set; }

		public OrderDetail OrderDetail { get; set; }
		public OrderStatusDetail OrderStatusDetail { get; set; }
		public OrderPairingDetail OrderPairingDetail { get; set; }
		public OrderPaymentDetail OrderPaymentDetail { get; set; }
		public OverduePaymentDetail OverduePaymentDetail { get; set; }

		public string RelatedEvents { get; set; }
	}
}
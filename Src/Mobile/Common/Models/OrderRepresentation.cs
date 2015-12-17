using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Models
{
	public class OrderRepresentation
	{
		public OrderRepresentation(Order order, OrderStatusDetail status)
		{
			Order = order;
			OrderStatus = status;
		}

		public Order Order { get; set; }
		public OrderStatusDetail OrderStatus { get; set; }
	}
}
using System;
using System.Runtime.Serialization;

namespace apcurium.MK.Booking.Mobile.AppServices.Orders
{
	public class OrderCreationException: Exception, ISerializable
	{
		public OrderCreationException(string message)
			:base(message)
		{
		}

		public OrderCreationException(string message, Exception inner)
			:base(message, inner)
		{
		}

		protected OrderCreationException(SerializationInfo info, StreamingContext context)
			:base(info, context)
		{
		}
	}
}


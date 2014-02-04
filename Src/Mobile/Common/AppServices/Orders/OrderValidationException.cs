using System;
using System.Runtime.Serialization;

namespace apcurium.MK.Booking.Mobile.AppServices.Orders
{
	public class OrderValidationException: Exception, ISerializable
	{
		public OrderValidationException(string message, OrderValidationError error)
			:base(message)
		{
			this.Error = error;
		}

		public OrderValidationException(string message, OrderValidationError error, Exception inner)
			:base(message, inner)
		{
			this.Error = error;
		}

		protected OrderValidationException(SerializationInfo info, StreamingContext context)
			:base(info, context)
		{
		}

		public OrderValidationError Error
		{
			get;
			private set;
		}
	}
}


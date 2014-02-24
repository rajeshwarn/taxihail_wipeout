using System;
using System.Runtime.Serialization;

namespace apcurium.MK.Booking.Mobile.AppServices.Orders
{
	public class OrderCreationException: Exception, ISerializable
	{
		public OrderCreationException(string message, string messageNoCall)
			:base(message)
		{
			this.MessageNoCall = messageNoCall;
		}

		public OrderCreationException(string message, string messageNoCall, Exception inner)
			:base(message, inner)
		{
			this.MessageNoCall = messageNoCall;
		}

		protected OrderCreationException(SerializationInfo info, StreamingContext context)
			:base(info, context)
		{
		}

		public string MessageNoCall
		{
			get;
			private set;
		}
	}
}


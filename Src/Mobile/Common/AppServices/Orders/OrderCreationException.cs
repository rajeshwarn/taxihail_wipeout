using System;
using System.Runtime.Serialization;

namespace apcurium.MK.Booking.Mobile.AppServices.Orders
{
	public class OrderCreationException: Exception, ISerializable
	{
        public OrderCreationException(string message)
            : base(message)
        {
        }

		public OrderCreationException(string message, string parameter)
			:base(message)
		{
            Parameter = parameter;
		}

        public OrderCreationException(string message, string parameter, Exception inner)
			:base(message, inner)
		{
            Parameter = parameter;
		}

		protected OrderCreationException(SerializationInfo info, StreamingContext context)
			:base(info, context)
		{
		}

        public string Parameter { get; private set; }
	}
}


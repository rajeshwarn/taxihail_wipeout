using System;
using System.Runtime.Serialization;

namespace apcurium.MK.Booking.Mobile.AppServices.Orders
{
	public class InvalidCreditCardException: Exception, ISerializable
	{
		public InvalidCreditCardException()
			: base()
		{
		}

		public InvalidCreditCardException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected InvalidCreditCardException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}

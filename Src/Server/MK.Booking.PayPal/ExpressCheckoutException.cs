using System;
using System.Runtime.Serialization;


namespace MK.Booking.PayPal
{
    public class ExpressCheckoutException : Exception
    {
        public ExpressCheckoutException()
        {
        }

        public ExpressCheckoutException(string message)
            : base(message)
        {
        }

        public ExpressCheckoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ExpressCheckoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
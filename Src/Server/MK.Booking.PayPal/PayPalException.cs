using System;
using System.Runtime.Serialization;


namespace MK.Booking.PayPal
{
    public class PayPalException : Exception
    {
        public PayPalException()
        {
        }

        public PayPalException(string message)
            : base(message)
        {
        }

        public PayPalException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PayPalException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
using System;
using System.Runtime.Serialization;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public class AuthException: Exception, ISerializable
    {
        public AuthException(string message, AuthFailure failure)
            :base(message)
        {
            this.Failure = failure;
        }

        public AuthException(string message, AuthFailure failure, Exception inner)
            :base(message, inner)
        {
            this.Failure = failure;
        }

        protected AuthException(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {
        }

        public AuthFailure Failure
        {
            get;
            private set;
        }
    }
}


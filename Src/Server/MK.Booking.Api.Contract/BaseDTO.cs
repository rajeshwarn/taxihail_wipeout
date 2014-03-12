#region

using System;

#endregion

namespace apcurium.MK.Booking.Api.Contract
{
    public class BaseDto
    {
        public override string ToString()
        {
            try
            {
                return JsonSerializer.SerializeToString(this, GetType());
            }
            catch (Exception e)
            {
                return string.Format("Can't serialize {0} : {1}", GetType().Name, e.StackTrace);
            }
        }
    }
}
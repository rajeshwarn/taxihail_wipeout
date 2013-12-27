using System;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class DateTimePicked : GenericTinyMessage<DateTime?>
    {
        public DateTimePicked(object sender, DateTime? datetime)
            : base(sender, datetime)
        {            
        }
    }
}
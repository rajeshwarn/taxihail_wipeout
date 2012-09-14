using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Requests;

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
using PushSharp.Apple;
using PushSharp.Blackberry;
using PushSharp.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.PushNotifications.Impl
{
    public class AppPushBrokers
    {
        public ApnsServiceBroker Apns { get; set; }
        public GcmServiceBroker Gcm { get; set; }
        public BlackberryServiceBroker Bb { get; set; }
    }

}
#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using apcurium.MK.Common.Enumeration;

#endregion

namespace apcurium.MK.Booking.ReadModel
{
    public class DeviceDetail
    {
        [Key, Column(Order = 1)]
        public Guid AccountId { get; set; }

        [Key, Column(Order = 2), MaxLength(1024)]
        public string DeviceToken { get; set; }

        public PushNotificationServicePlatform Platform { get; set; }
    }
}
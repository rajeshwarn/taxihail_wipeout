#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace apcurium.MK.Booking.ReadModel
{
    public class OrderStatusUpdateDetail
    {
        [Key]
        public Guid Id { get; set; }

        public string UpdaterUniqueId { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
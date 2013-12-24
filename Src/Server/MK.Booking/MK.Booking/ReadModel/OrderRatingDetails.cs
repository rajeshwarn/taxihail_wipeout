#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace apcurium.MK.Booking.ReadModel
{
    public class OrderRatingDetails
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public string Note { get; set; }
    }
}
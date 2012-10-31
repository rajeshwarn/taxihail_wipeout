using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

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

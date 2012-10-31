using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.ReadModel
{
    public class RatingScoreDetails
    {
        [Key]
        public Guid Id;
        public Guid OrderId { get; set; }
        public Guid RatingTypeId { get; set; }
        public int Score { get; set; }
    }
}

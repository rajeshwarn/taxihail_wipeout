using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class RatingScoreDetails
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Guid RatingTypeId { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
    }
}
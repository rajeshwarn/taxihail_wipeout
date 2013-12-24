using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class RatingTypeDetail
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public bool IsHidden { get; set; }
    }
}
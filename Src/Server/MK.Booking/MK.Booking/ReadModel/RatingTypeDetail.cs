#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace apcurium.MK.Booking.ReadModel
{
    public class RatingTypeDetail
    {
        public Guid Id { get; set; }

        public string Language { get; set; }

        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public bool IsHidden { get; set; }
        
    }
}
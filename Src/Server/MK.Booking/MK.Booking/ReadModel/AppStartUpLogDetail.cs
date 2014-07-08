using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class AppStartUpLogDetail
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Email { get; set; }

        public DateTime DateOccured { get; set; }

        public string ApplicationVersion { get; set; }

        public string Platform { get; set; }

        public string PlatformDetails { get; set; }

        public string ServerVersion { get; set; }
    }
}
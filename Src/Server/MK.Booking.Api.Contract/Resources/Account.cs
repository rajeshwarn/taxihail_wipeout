using System;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class Account : BaseDto
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public int IbsAccountid { get; set; }

        public string FacebookId { get; set; }

        public string TwitterId { get; set; }

        public BookingSettings Settings { get; set; }

        public string Language { get; set; }

        public bool IsAdmin { get; set; }
        public bool IsSuperAdmin { get; set; }

        public Guid? DefaultCreditCard { get; set; }

        public int? DefaultTipPercent { get; set; }
    }
}
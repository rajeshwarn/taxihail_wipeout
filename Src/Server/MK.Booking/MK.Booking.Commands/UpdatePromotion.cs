using System;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdatePromotion : ICommand
    {
        public UpdatePromotion()
        {
            Id = Guid.NewGuid();
        }

        public Guid PromoId { get; set; }

        public string Name { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DayOfWeek[] DaysOfWeek { get; set; }

        public bool AppliesToCurrentBooking { get; set; }

        public bool AppliesToFutureBooking { get; set; }

        public double DiscountValue { get; set; }

        public PromoDiscountType DiscountType { get; set; }

        public int? MaxUsagePerUser { get; set; }

        public int? MaxUsage { get; set; }

        public string Code { get; set; }

        public Guid Id { get; private set; }
    }
}
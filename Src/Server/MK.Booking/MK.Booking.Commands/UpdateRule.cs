using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateRule : ICommand
    {
        public UpdateRule()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid RuleId { get; set; }
        public string Name { get; set; }
        public string ZoneList { get; set; }
        public bool AppliesToCurrentBooking { get; set; }
        public bool AppliesToFutureBooking { get; set; }
        public DayOfTheWeek DaysOfTheWeek { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public int Priority { get; set; }
        public string Message { get; set; }
        public RuleCategory Category { get; set; }
        public RuleType Type { get; set; }
        public bool IsActive { get; set; }

    }
}

﻿using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateRule : ICommand
    {
        public UpdateRule()
        {
            Id = Guid.NewGuid();
        }

        public Guid CompanyId { get; set; }

        public Guid RuleId { get; set; }

        public string Name { get; set; }

        public string ZoneList { get; set; }

        public bool ZoneRequired { get; set; }

		public bool ExcludeCircularZone { get; set; }

		public double ExcludedCircularZoneLatitude { get; set; }

		public double ExcludedCircularZoneLongitude { get; set; }

		public int ExcludedCircularZoneRadius { get; set; }

        public bool AppliesToCurrentBooking { get; set; }

        public bool AppliesToFutureBooking { get; set; }

        public bool AppliesToPickup { get; set; }

        public bool AppliesToDropoff { get; set; }

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

        public Guid Id { get; set; }

        public string Market { get; set; }

        public bool DisableFutureBookingOnError { get; set; }
    }
}
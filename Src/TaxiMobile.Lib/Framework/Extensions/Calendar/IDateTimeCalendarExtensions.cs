﻿using System;
using System.Collections.Generic;

namespace TaxiMobile.Lib.Framework.Extensions.Calendar
{
	public interface IDateTimeCalendarExtensions
	{
		DateTime Now { get; }

		DateTime UtcNow { get; }

		DateTime Today { get; }

		DayOfWeek WeekBeginsOn { get; }

		DayOfWeek WeekEndsOn { get; }

		IEnumerable<int> QuarterMonths { get; }
	}
}

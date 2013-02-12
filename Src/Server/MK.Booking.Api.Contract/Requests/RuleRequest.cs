﻿using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    

    [Authenticate]
#if !CLIENT
    [AuthorizationRequired(ApplyTo.Post | ApplyTo.Put | ApplyTo.Delete, Permissions.Admin)]
#endif
    [RestService("/admin/rules", "GET, POST")]
    [RestService("/admin/rules/{Id}", "PUT, DELETE")]
    public class RuleRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
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

    public class RuleResponse : IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }

}

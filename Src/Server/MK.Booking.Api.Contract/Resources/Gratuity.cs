#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class Gratuity : BaseDto
    {
        public Guid OrderId { get; set; }
        public int Percentage { get; set; }
        
    }
}
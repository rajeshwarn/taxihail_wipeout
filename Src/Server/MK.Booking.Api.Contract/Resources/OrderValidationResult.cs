using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class OrderValidationResult
    {
        public bool HasWarning { get; set; }

        public string Message{ get; set; }
    }
}

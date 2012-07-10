using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace apcurium.MK.Common.Enumeration
{
    [Flags]
    public enum OrderStatus
    {
        Created,
        Cancelled,
        Completed
    }
}

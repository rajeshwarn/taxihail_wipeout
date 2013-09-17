using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Security
{
    public class RoleName
    {
        public const string Admin = "Admin";
        public const string SuperAdmin = "SuperAdmin";
    }

    [Flags]
    public enum Roles
    {
        Admin = 1,
        SuperAdmin = 3,
    }
}

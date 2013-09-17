using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Security
{
    public class Permissions
    {
        public const string Admin = "Admin";
    }

    [Flags]
    public enum Roles
    {
        Admin = 1,
        SuperAdmin = 3,
    }
}

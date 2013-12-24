#region

using System;

#endregion

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
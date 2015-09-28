#region

using System;

#endregion

namespace apcurium.MK.Booking.Security
{
    public class RoleName
    {
        public const string Admin = "Admin";
        public const string SuperAdmin = "SuperAdmin";
        public const string Support = "Support";
    }

    [Flags]
    public enum Roles
    {
        Admin = 1,
        SuperAdmin = 3,
        Support = 5,
    }

    public enum OldRoles
    {
        Admin = 1,
        SuperAdmin = 3,
    }
}
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
        Support = 4,
        //We use those values in order to be able to add Roles before Admin (Support in the basic role)
        //Admin has all Support permissions and SuperAdmin has both Support and Admin persmissions
        Admin = 0xFF,
        SuperAdmin = 0x1FF,
    }

    public enum OldRoles
    {
        Admin = 1,
        SuperAdmin = 3,
    }
}
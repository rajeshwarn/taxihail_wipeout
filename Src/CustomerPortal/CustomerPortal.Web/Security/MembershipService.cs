#region

using System;
using System.Web.Security;
using WebMatrix.WebData;

#endregion

namespace CustomerPortal.Web.Security
{
    public class MembershipService : IMembershipService
    {
        public string CreateUserAndAccount(string emailAddress, string password, object propertyValues)
        {
            return WebSecurity.CreateUserAndAccount(emailAddress, password, propertyValues);
        }

        public bool DeleteAccount(string username)
        {
            return ((ExtendedMembershipProvider) Membership.Provider).DeleteAccount(username);
        }

        public void AddUserToRole(string emailAddress, string roleName)
        {
            Roles.AddUserToRole(emailAddress, roleName);
        }

        public bool ChangePassword(string emailAddress, string currentPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public bool SignIn(string email, string password, bool persistCookie = false)
        {
            throw new NotImplementedException();
        }

        public void SignOut()
        {
            throw new NotImplementedException();
        }
    }

    public interface IMembershipService
    {
        string CreateUserAndAccount(string emailAddress, string password, object propertyValues);
        void AddUserToRole(string emailAddress, string roleName);
        bool ChangePassword(string emailAddress, string currentPassword, string newPassword);
        bool SignIn(string email, string password, bool persistCookie = false);
        void SignOut();

        bool DeleteAccount(string username);
    }
}
#region

using System;
using System.Collections.Generic;
using System.Linq;
using CustomerPortal.Web.Security;

#endregion

namespace CustomerPortal.Web.Test.Helpers.Membership
{
    public class InMemoryMembershipService : IMembershipService
    {
        public InMemoryMembershipService()
        {
            Users = new List<User>();
        }

        public IList<User> Users { get; private set; }

        public string CreateUserAndAccount(string emailAddress, string password, object propertyValues)
        {
            Users.Add(new User
            {
                EmailAddress = emailAddress,
                Password = password,
            });
            return emailAddress;
        }

        public bool DeleteAccount(string username)
        {
            var user = Users.SingleOrDefault(x => x.EmailAddress == username);
            if (user == null) return false;

            Users.Remove(user);
            return true;
        }

        public void AddUserToRole(string emailAddress, string roleName)
        {
            Users.Single(x => x.EmailAddress == emailAddress).Roles.Add(roleName);
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
        }

        public class User
        {
            public User()
            {
                Roles = new List<string>();
            }

            public string EmailAddress { get; set; }
            public string Password { get; set; }
            public List<string> Roles { get; set; }
        }
    }
}
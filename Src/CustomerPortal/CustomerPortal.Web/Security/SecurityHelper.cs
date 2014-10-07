#region

using WebMatrix.WebData;

#endregion

namespace CustomerPortal.Web.Security
{
    public class SecurityHelper
    {
        public static bool IsApcuriumUser
        {
            get
            {
                return !string.IsNullOrEmpty(WebSecurity.CurrentUserName) &&
                       (WebSecurity.CurrentUserName.ToLower().EndsWith("@apcurium.com"));
            }
        }
    }
}
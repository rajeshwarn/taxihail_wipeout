#region

using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using ExtendedMongoMembership;
using MongoDB.Bson;
using WebMatrix.WebData;

#endregion

namespace CustomerPortal.Web.Security
{
    public class CustomerCompanyFilterAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            string companyId = null;
            if (!WebSecurity.IsAuthenticated)
            {
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }
            if (Roles.IsUserInRole(RoleName.Admin))
            {
                companyId = (string) filterContext.HttpContext.Session[SessionKey.AdminCurrentCompany];
            }
            else
            {
                var session =
                    new MongoSession(ConfigurationManager.ConnectionStrings["MongoServerSettings"].ConnectionString);
                var user = session.Users.Single(x => x.UserId == WebSecurity.CurrentUserId);
                var value = user.GetValue("Company", default(BsonString));
                companyId = value == null ? null : value.ToString();
            }

            if (companyId == null)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
            else
            {
                filterContext.RouteData.Values["companyId"] = companyId;
            }
        }
    }
}
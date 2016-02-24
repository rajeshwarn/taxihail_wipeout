using System.Web.Mvc;
using apcurium.MK.Web.Areas;

namespace apcurium.MK.Web.Attributes
{
    public class AuthorizationRequiredAttribute : FilterAttribute, IAuthorizationFilter
    {
        private readonly string[] _permissions;

        public AuthorizationRequiredAttribute(params string[] permissions)
        {
            _permissions = permissions;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!UserHasPermission(filterContext))
            {
                filterContext.Result = new HttpUnauthorizedResult(string.Format("You do not have the required permissions: {0}", _permissions));
            }
        }

        private bool UserHasPermission(AuthorizationContext filterContext)
        {
            return ((ApcuriumServiceController) filterContext.Controller).UserHasPermission(_permissions);
        }
    }
}
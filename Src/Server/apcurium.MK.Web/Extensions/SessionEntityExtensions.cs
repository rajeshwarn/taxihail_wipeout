using System.Linq;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Http;
using Microsoft.Practices.Unity;
using UnityServiceContainer = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Booking.Api.Extensions
{
    public static class SessionEntityExtensions
    {
        public static bool HasPermission(this SessionEntity source, string permission)
        {
            var dao = UnityServiceContainer.Instance.Resolve<IAccountDao>();

            var account = dao.FindById(source.UserId);

            return account.RoleNames.Any(p => p == permission);
        }
    }
}

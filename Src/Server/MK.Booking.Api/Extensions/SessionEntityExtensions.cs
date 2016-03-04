using System.Linq;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Extensions;
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

        public static void RemoveSessionIfNeeded(this SessionEntity source)
        {
            if (source == null || !source.IsAuthenticated())
            {
                return;
            }

            var dao = UnityServiceContainer.Instance.Resolve<ICacheClient>();

            var urn = "urn:iauthsession:{0}".InvariantCultureFormat(source.SessionId);

            dao.Remove(urn);
        }
    }
}

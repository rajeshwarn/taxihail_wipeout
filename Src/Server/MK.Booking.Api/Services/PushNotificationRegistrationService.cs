#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using System.Linq;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class PushNotificationRegistrationService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _accountDao;
        private readonly IDeviceDao _deviceDao;

        public PushNotificationRegistrationService(IAccountDao accountDao,
                                                    IDeviceDao deviceDao,
                                                    ICommandBus commandBus)
        {
            _accountDao = accountDao;
            _deviceDao = deviceDao;
            _commandBus = commandBus;
        }

        public object Post(PushNotificationRegistration request)
        {
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            _deviceDao.Add(account.Id, request.DeviceToken, request.Platform);

            return new HttpResult(HttpStatusCode.OK);
        }

        public object Delete(PushNotificationRegistration request)
        {
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            _deviceDao.Remove(account.Id, request.DeviceToken);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
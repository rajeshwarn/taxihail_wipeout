#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class PushNotificationRegistrationService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _dao;

        public PushNotificationRegistrationService(IAccountDao dao, ICommandBus commandBus)
        {
            _dao = dao;
            _commandBus = commandBus;
        }

        public object Post(PushNotificationRegistration request)
        {
            var account = _dao.FindById(new Guid(this.GetSession().UserAuthId));

            var command = new RegisterDeviceForPushNotifications
            {
                AccountId = account.Id,
                DeviceToken = request.DeviceToken,
                OldDeviceToken = request.OldDeviceToken,
                Platform = request.Platform
            };

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public object Delete(PushNotificationRegistration request)
        {
            var account = _dao.FindById(new Guid(this.GetSession().UserAuthId));
            var command = new UnregisterDeviceForPushNotifications
            {
                AccountId = account.Id,
                DeviceToken = request.DeviceToken,
            };

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
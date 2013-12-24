using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class PushNotificationRegistrationService : RestServiceBase<PushNotificationRegistration>
    {
        private readonly IAccountDao _dao;
        private readonly ICommandBus _commandBus;

        public PushNotificationRegistrationService(IAccountDao Dao, ICommandBus commandBus)
        {
            _dao = Dao;
            _commandBus = commandBus;
        }

        public override object OnPost(PushNotificationRegistration request)
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

        public override object OnDelete(PushNotificationRegistration request)
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

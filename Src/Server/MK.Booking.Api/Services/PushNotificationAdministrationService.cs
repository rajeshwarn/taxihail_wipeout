using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.Api.Services
{
    public class PushNotificationAdministrationService : RestServiceBase<PushNotificationAdministrationRequest>
    {
        private readonly IAccountDao _dao;
        private readonly IDeviceDao _daoDevice;
        private readonly ICommandBus _commandBus;
        private readonly IPushNotificationService _pushNotificationService;

        public PushNotificationAdministrationService(IAccountDao Dao, IDeviceDao Device, ICommandBus commandBus, IPushNotificationService pushNotificationService )
        {
            _dao = Dao;
            _daoDevice = Device;
            _commandBus = commandBus;
            _pushNotificationService = pushNotificationService;
        }

        public override object OnPost(PushNotificationAdministrationRequest request)
        {
            var account = _dao.FindByEmail(request.EmailAddress);

            if (account == null)
            {
                throw new HttpError(HttpStatusCode.InternalServerError, "sendPushNotificationErrorNoAccount");                
            }                
            
            var devices = _daoDevice.FindByAccountId(account.Id);
            
            if (devices == null || !devices.Any())
            {
                throw new HttpError(HttpStatusCode.InternalServerError, "sendPushNotificationErrorNoDevice");                
            }
            
            foreach (DeviceDetail device in devices)
            {
                try
                {
                    _pushNotificationService.Send(request.Message, new Dictionary<string, object>(), device.DeviceToken,
                        device.Platform);

                }catch(Exception e)
                {
                    throw new HttpError(HttpStatusCode.InternalServerError, e.Message); 
                }
            }            

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}

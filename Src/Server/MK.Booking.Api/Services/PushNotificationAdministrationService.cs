#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class PushNotificationAdministrationService : Service
    {
        private readonly IAccountDao _dao;
        private readonly IDeviceDao _daoDevice;
        private readonly ILogger _logger;
        private readonly IPushNotificationService _pushNotificationService;

        public PushNotificationAdministrationService(IAccountDao dao, IDeviceDao device,
            IPushNotificationService pushNotificationService, ILogger logger)
        {
            _dao = dao;
            _daoDevice = device;
            _logger = logger;
            _pushNotificationService = pushNotificationService;
        }

        public object Post(PushNotificationAdministrationRequest request)
        {
            var account = _dao.FindByEmail(request.EmailAddress);

            if (account == null)
            {
                throw new HttpError(HttpStatusCode.InternalServerError, "sendPushNotificationErrorNoAccount");
            }

            var devices = _daoDevice.FindByAccountId(account.Id);

            var deviceDetails = devices as DeviceDetail[] ?? devices.ToArray();
            if (devices == null || !deviceDetails.Any())
            {
                throw new HttpError(HttpStatusCode.InternalServerError, "sendPushNotificationErrorNoDevice");
            }

            foreach (var device in deviceDetails)
            {
                try
                {
                    _pushNotificationService.Send(request.Message, new Dictionary<string, object>(), device.DeviceToken,
                        device.Platform);
                }
                catch (Exception e)
                {
                    _logger.LogError(e);
                    throw new HttpError(HttpStatusCode.InternalServerError, device.Platform + "-" + e.Message);
                }
            }

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
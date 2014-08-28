#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.PushNotifications.Impl;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
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
        private readonly IConfigurationManager _configurationManager;

        public PushNotificationAdministrationService(IAccountDao dao, IDeviceDao device,
                    ILogger logger, IConfigurationManager configurationManager)
        {
            _dao = dao;
            _daoDevice = device;
            _logger = logger;
            _configurationManager = configurationManager;
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

            // We create a new instance each time as we need to start from a clean state to get meaningful error messages
            var pushNotificationService = new PushNotificationService(_configurationManager, _logger);

            foreach (var device in deviceDetails)
            {
                try
                {
                    pushNotificationService.Send(request.Message, new Dictionary<string, object>(), device.DeviceToken,
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
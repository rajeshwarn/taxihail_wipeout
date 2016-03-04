#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class PushNotificationRegistrationService : BaseApiService
    {
        private readonly IAccountDao _accountDao;
        private readonly IDeviceDao _deviceDao;

        public PushNotificationRegistrationService(IAccountDao accountDao, IDeviceDao deviceDao)
        {
            _accountDao = accountDao;
            _deviceDao = deviceDao;
        }

        public void Post(PushNotificationRegistration request)
        {
            var account = _accountDao.FindById(Session.UserId);

            _deviceDao.Add(account.Id, request.DeviceToken, request.Platform);
        }

        public void Delete(string deviceToken)
        {
            var account = _accountDao.FindById(Session.UserId);

            _deviceDao.Remove(account.Id, deviceToken);
        }
    }
}
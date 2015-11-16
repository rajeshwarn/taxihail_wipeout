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
        private readonly IAccountDao _dao;
        private readonly Func<BookingDbContext> _contextFactory;

        public PushNotificationRegistrationService(IAccountDao dao, 
                                                    ICommandBus commandBus,
                                                    Func<BookingDbContext> contextFactory)
        {
            _dao = dao;
            _commandBus = commandBus;
            _contextFactory = contextFactory;
        }

        public object Post(PushNotificationRegistration request)
        {
            var account = _dao.FindById(new Guid(this.GetSession().UserAuthId));

            using (var context = _contextFactory.Invoke())
            {
                var devices = context.Set<DeviceDetail>().Where(d => d.DeviceToken == request.DeviceToken);

                context.Set<DeviceDetail>().RemoveRange(devices.Where(d => d.AccountId != account.Id));

                if (devices.None(d => d.AccountId == account.Id))
                {
                    var device = new DeviceDetail
                    {
                        AccountId = account.Id,
                        DeviceToken = request.DeviceToken,
                        Platform = request.Platform
                    };
                    context.Set<DeviceDetail>().Add(device);

                }

                context.SaveChanges();
            }

            return new HttpResult(HttpStatusCode.OK);
        }

        public object Delete(PushNotificationRegistration request)
        {
            var account = _dao.FindById(new Guid(this.GetSession().UserAuthId));

            using (var context = _contextFactory.Invoke())
            {
                var device = context.Set<DeviceDetail>().Find(account.Id, request.DeviceToken);
                if (device != null)
                {
                    context.Set<DeviceDetail>().Remove(device);
                    context.SaveChanges();
                }
            }

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
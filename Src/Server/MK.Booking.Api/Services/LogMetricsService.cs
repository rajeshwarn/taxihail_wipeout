using System;
using System.Reflection;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class LogMetricsService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _dao;
        private readonly IOrderDao _orderDao;

        public LogMetricsService(ICommandBus commandBus, IAccountDao dao, IOrderDao orderDao)
        {
            _commandBus = commandBus;
            _dao = dao;
            _orderDao = orderDao;
        }

        public void Post(LogApplicationStartUpRequest request)
        {
            var session = this.GetSession();
            var account = _dao.FindById(new Guid(session.UserAuthId));

            var command = new LogApplicationStartUp
            {
                UserId = account.Id,
                Email = account.Email,
                DateOccured = request.StartUpDate,
                ApplicationVersion = request.ApplicationVersion,
                Platform = request.Platform,
                PlatformDetails = request.PlatformDetails,
                ServerVersion = Assembly.GetAssembly(typeof(ApplicationInfoService)).GetName().Version.ToString(),
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };

            _commandBus.Send(command);
        }

        public void Post(LogOriginalEtaRequest request)
        {
            var order = _orderDao.FindOrderStatusById(request.OrderId);

            if (order != null && !order.OriginalEta.HasValue && request.OriginalEta.HasValue)
            {
                _commandBus.Send(new SaveOriginalEta
                {
                    OrderId = request.OrderId,
                    OriginalEta = request.OriginalEta.Value
                });
            }
        }
    }
}
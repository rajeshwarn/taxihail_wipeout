using System;
using System.Reflection;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class LogApplicationStartUpService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _dao;

        public LogApplicationStartUpService(ICommandBus commandBus, IAccountDao dao)
        {
            _commandBus = commandBus;
            _dao = dao;
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
                ServerVersion = Assembly.GetAssembly(typeof(ApplicationInfoService)).GetName().Version.ToString()
            };

            _commandBus.Send(command);

            
        }
    }
}
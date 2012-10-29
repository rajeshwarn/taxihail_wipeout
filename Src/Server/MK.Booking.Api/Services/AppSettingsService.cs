using System;
using System.Net;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Services
{
    public class AppSettingsService : RestServiceBase<AppSettingsRequest>
    {

        private readonly ICommandBus _commandBus;
        protected IConfigurationManager Dao { get; set; }

        public AppSettingsService(IConfigurationManager dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            Dao = dao;
        }

        public override object OnPost(AppSettingsRequest request)
        {

                var command = new Commands.AddAppSettings {Key = request.Key, Value = request.Value};
                _commandBus.Send(command);
                return new HttpResult(HttpStatusCode.OK);
        }

        public override object OnGet(AppSettingsRequest request)
        {
            return Dao.GetAllSettings();
        }

        public override object OnPut(AppSettingsRequest request)
        {

            var setting = Dao.GetSetting(request.Key);

            if (setting!=null)
            {
                var command = new Commands.UpdateAppSettings {Key = request.Key, Value = request.Value};
                _commandBus.Send(command);
                return new HttpResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpResult(HttpStatusCode.Conflict);
            }
        }
    }
}
            

            
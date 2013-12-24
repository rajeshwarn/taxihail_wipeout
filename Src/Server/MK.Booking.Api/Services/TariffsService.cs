#region

using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class TariffsService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly ITariffDao _dao;

        public TariffsService(ITariffDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            _dao = dao;
        }

        public object Get(Tariff request)
        {
            return _dao.GetAll();
        }

        public object Post(Tariff request)
        {
            //Check if rate with same name already exists
            if (_dao.GetAll().Any(x => x.Name == request.Name))
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.Tariff_DuplicateName.ToString());
            }

            var command = Mapper.Map<CreateTariff>(request);

            _commandBus.Send(command);

            return new
            {
                Id = command.TariffId
            };
        }

        public object Put(Tariff request)
        {
            //Check if rate with same name already exists
            if (_dao.GetAll().Any(x => x.Id != request.Id && x.Name == request.Name))
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.Tariff_DuplicateName.ToString());
            }

            var command = Mapper.Map<UpdateTariff>(request);

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }

        public object Delete(Tariff request)
        {
            var command = new DeleteTariff {CompanyId = AppConstants.CompanyId, TariffId = request.Id};
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }
}
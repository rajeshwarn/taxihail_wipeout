using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Api.Services
{
    public class TariffsService : RestServiceBase<Tariff>
    {
        private readonly ICommandBus _commandBus;
        private readonly ITariffDao _dao;

        public TariffsService(ITariffDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            _dao = dao;
        }



        public override object OnGet(Tariff request)
        {
            return _dao.GetAll();
        }

        public override object OnPost(Tariff request)
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

        public override object OnPut(Tariff request)
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

        public override object OnDelete(Tariff request)
        {
            var command = new DeleteTariff {CompanyId = AppConstants.CompanyId, TariffId = request.Id};
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }

}

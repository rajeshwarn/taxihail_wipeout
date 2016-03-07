#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using AutoMapper;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class TariffsService : BaseApiService
    {
        private readonly ICommandBus _commandBus;
        private readonly ITariffDao _dao;

        public TariffsService(ITariffDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            _dao = dao;
        }

        public IList<TariffDetail> Get()
        {
            return _dao.GetAll();
        }

        public object Post(Tariff request)
        {
            //Check if rate with same name already exists
            if (_dao.GetAll().Any(x => x.Name == request.Name))
            {
                throw new HttpException((int)HttpStatusCode.Conflict, ErrorCode.Tariff_DuplicateName.ToString());
            }

            var command = Mapper.Map<CreateTariff>(request);

            _commandBus.Send(command);

            return new
            {
                Id = command.TariffId
            };
        }

        public void Put(Tariff request)
        {
            //Check if rate with same name already exists
            if (_dao.GetAll().Any(x => x.Id != request.Id && x.Name == request.Name))
            {
                throw new HttpException((int)HttpStatusCode.Conflict, ErrorCode.Tariff_DuplicateName.ToString());
            }

            var command = Mapper.Map<UpdateTariff>(request);

            _commandBus.Send(command);
        }

        public void Delete(Guid id)
        {
            var command = new DeleteTariff {CompanyId = AppConstants.CompanyId, TariffId = id};
            _commandBus.Send(command);
            
        }
    }
}
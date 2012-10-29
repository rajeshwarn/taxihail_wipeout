using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
    public class RatesService : RestServiceBase<Rates>
    {
        private readonly ICommandBus _commandBus;
        private readonly IRateDao _dao;

        public RatesService(IRateDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            _dao = dao;
        }



        public override object OnGet(Rates request)
        {
            return _dao.GetAll();
        }

        public override object OnPost(Rates request)
        {
            //Check if rate with same name already exists
            if (_dao.GetAll().Any(x => x.Name == request.Name))
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.Rate_DuplicateName.ToString());
            }

            var command = Mapper.Map<CreateRate>(request);

            _commandBus.Send(command);

            return new
            {
                Id = command.RateId
            };
        }

        public override object OnPut(Rates request)
        {
            //Check if rate with same name already exists
            if (_dao.GetAll().Any(x => x.Id != request.Id && x.Name == request.Name))
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.Rate_DuplicateName.ToString());
            }

            var command = Mapper.Map<UpdateRate>(request);

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }

        public override object OnDelete(Rates request)
        {
            var command = new DeleteRate {CompanyId = AppConstants.CompanyId, RateId = request.Id};
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }

}

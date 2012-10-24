using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class RatesService : RestServiceBase<Rates>
    {
        readonly ICommandBus _commandBus;
        readonly IRateDao _dao;
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
           var command = Mapper.Map<CreateRate>(request);

           _commandBus.Send(command);

            return new
            {
                Id = command.RateId
            };
        }
    }
}

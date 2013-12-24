using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace apcurium.MK.Booking.Api.Services
{

    public class RulesService : RestServiceBase<RuleRequest>
    {
        private readonly ICommandBus _commandBus;
        private readonly IRuleDao _dao;

        public RulesService(IRuleDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            _dao = dao;
        }



        public override object OnGet(RuleRequest request)
        {
            return _dao.GetAll();
        }

        public override object OnPost(RuleRequest request)
        {
            //Check if rate with same name already exists
            if ( ( request.Type != Common.Entity.RuleType.Default ) && (_dao.GetAll().Any(x => x.Name == request.Name)))
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.Rule_DuplicateName.ToString());
            }
            if (_dao.GetAll().Any(x => x.Priority == request.Priority && x.Category == (int)request.Category))
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.Rule_InvalidPriority.ToString());
            }

            var command = Mapper.Map<CreateRule>(request);

            _commandBus.Send(command);

            
            return new
            {
                Id = command.RuleId
            };
        }

        public override object OnPut(RuleRequest request)
        {
            //Check if rate with same name already exists
            if ( ( request.Type != Common.Entity.RuleType.Default ) && (_dao.GetAll().Any(x => x.Id != request.Id && x.Name == request.Name)) )
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.Rule_DuplicateName.ToString());
            }
            //if (_dao.GetAll().Any(x => x.Priority == request.Priority && x.Category == (int)request.Category))
            if (_dao.GetAll().Where(x => x.Id != request.Id).Any(x => x.Priority == request.Priority && x.Category == (int)request.Category))
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.Rule_InvalidPriority.ToString());
            }

            var command = Mapper.Map<UpdateRule>(request);

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }

        public override object OnDelete(RuleRequest request)
        {
            var command = new DeleteRule {CompanyId = AppConstants.CompanyId, RuleId= request.Id};
            
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }

    }
}

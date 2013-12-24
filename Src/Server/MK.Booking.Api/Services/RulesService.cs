#region

using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class RulesService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IRuleDao _dao;

        public RulesService(IRuleDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            _dao = dao;
        }


        public object Get(RuleRequest request)
        {
            return _dao.GetAll();
        }

        public object Post(RuleRequest request)
        {
            //Check if rate with same name already exists
            if ((request.Type != RuleType.Default) && (_dao.GetAll().Any(x => x.Name == request.Name)))
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.Rule_DuplicateName.ToString());
            }
            if (_dao.GetAll().Any(x => x.Priority == request.Priority && x.Category == (int) request.Category))
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

        public object Put(RuleRequest request)
        {
            //Check if rate with same name already exists
            if ((request.Type != RuleType.Default) &&
                (_dao.GetAll().Any(x => x.Id != request.Id && x.Name == request.Name)))
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.Rule_DuplicateName.ToString());
            }
            //if (_dao.GetAll().Any(x => x.Priority == request.Priority && x.Category == (int)request.Category))
            if (
                _dao.GetAll()
                    .Where(x => x.Id != request.Id)
                    .Any(x => x.Priority == request.Priority && x.Category == (int) request.Category))
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.Rule_InvalidPriority.ToString());
            }

            var command = Mapper.Map<UpdateRule>(request);

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }

        public object Delete(RuleRequest request)
        {
            var command = new DeleteRule {CompanyId = AppConstants.CompanyId, RuleId = request.Id};

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }
}
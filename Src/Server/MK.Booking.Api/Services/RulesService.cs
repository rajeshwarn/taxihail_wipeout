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
using apcurium.MK.Common.Entity;
using AutoMapper;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class RulesService : BaseApiService
    {
        private readonly ICommandBus _commandBus;
        private readonly IRuleDao _dao;

        public RulesService(IRuleDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            _dao = dao;
        }


        public IList<RuleDetail> Get()
        {
            return _dao.GetAll();
        }

        public object Post(RuleRequest request)
        {
			if (request.ZoneRequired && request.ExcludeCircularZone)
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, ErrorCode.Rule_TwoTypeZoneVerificationSelected.ToString());
			}

            //Check if rate with same name already exists
            if ((request.Type != RuleType.Default) && (_dao.GetAll().Any(x => x.Name == request.Name)))
            {
                throw new HttpException((int)HttpStatusCode.Conflict, ErrorCode.Rule_DuplicateName.ToString());
            }
            if (_dao.GetAll().Any(x => x.Priority == request.Priority && x.Category == (int) request.Category))
            {
                throw new HttpException((int)HttpStatusCode.Conflict, ErrorCode.Rule_InvalidPriority.ToString());
            }

            var command = Mapper.Map<CreateRule>(request);

            _commandBus.Send(command);

            return new
            {
                Id = command.RuleId
            };
        }

        public void Put(RuleRequest request)
        {
			if (request.ZoneRequired && request.ExcludeCircularZone)
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, ErrorCode.Rule_TwoTypeZoneVerificationSelected.ToString());
			}

            //Check if rate with same name already exists
            if ((request.Type != RuleType.Default) &&
                (_dao.GetAll().Any(x => x.Id != request.Id && x.Name == request.Name)))
            {
                throw new HttpException((int)HttpStatusCode.Conflict, ErrorCode.Rule_DuplicateName.ToString());
            }
            //if (_dao.GetAll().Any(x => x.Priority == request.Priority && x.Category == (int)request.Category))
            if (
                _dao.GetAll()
                    .Where(x => x.Id != request.Id)
                    .Any(x => x.Priority == request.Priority && x.Category == (int) request.Category))
            {
                throw new HttpException((int)HttpStatusCode.Conflict, ErrorCode.Rule_InvalidPriority.ToString());
            }

            var command = Mapper.Map<UpdateRule>(request);

            _commandBus.Send(command);
        }

        public void Delete(Guid ruleId)
        {
            var command = new DeleteRule {CompanyId = AppConstants.CompanyId, RuleId = ruleId };

            _commandBus.Send(command);
        }
    }
}
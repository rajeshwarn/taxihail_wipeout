#region

using System;
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
    public class RatingTypesService : Service
    {
        private readonly IRatingTypeDao _dao;
        private readonly ICommandBus _commandBus;

        public RatingTypesService(IRatingTypeDao dao, ICommandBus commandBus)
        {
            _dao = dao;
            _commandBus = commandBus;
        }

        public object Get(RatingTypesRequest request)
        {
            var allRatingTypes = _dao.GetAll();
            return allRatingTypes.Where(r => r.Language == request.Language).ToList();
        }

        public object Post(RatingTypesRequest request)
        {
            var existing = _dao.FindByName(request.Name, request.Language);
            if (existing != null)
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.RatingType_DuplicateName.ToString());
            }

            var addRatingType = new AddRatingType
            {
                RatingTypeId = Guid.NewGuid(),
                CompanyId = AppConstants.CompanyId,
                Name = request.Name,
                Language = request.Language
            };

            _commandBus.Send(addRatingType);

            return new
            {
                Id = addRatingType.RatingTypeId
            };
        }

        public object Put(RatingTypesRequest request)
        {
            if (_dao.GetAll().Any(x => x.Id != request.Id && x.Name == request.Name && x.Language == request.Language))
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.RatingType_DuplicateName.ToString());
            }

            var command = Mapper.Map<UpdateRatingType>(request);

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }

        public object Delete(RatingTypesRequest request)
        {
            var command = new DeleteRatingType { CompanyId = AppConstants.CompanyId, RatingTypeId = request.Id };
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }
}
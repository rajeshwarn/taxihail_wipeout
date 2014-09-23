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
            return allRatingTypes.Where(r => r.Language == request.ClientLanguage).ToList();
        }

        public object Post(RatingTypesRequest request)
        {
            var ratingTypeId = Guid.NewGuid();

            foreach (var ratingType in request.RatingTypes)
            {
                var existing = _dao.FindByName(ratingType.Name, ratingType.Language);
                if (existing != null)
                {
                    continue;
                }

                var addRatingType = new AddRatingType
                {
                    RatingTypeId = ratingTypeId,
                    CompanyId = AppConstants.CompanyId,
                    Name = ratingType.Name,
                    Language = ratingType.Language
                };

                _commandBus.Send(addRatingType);
            }

            return new
            {
                Id = ratingTypeId
            };
        }

        public object Put(RatingTypesRequest request)
        {
            foreach (var ratingTypes in request.RatingTypes)
            {
                if (_dao.GetAll().Any(x => x.Id != request.Id && x.Name == ratingTypes.Name && x.Language == ratingTypes.Language))
                {
                    continue;
                }

                var command = Mapper.Map<UpdateRatingType>(request);

                _commandBus.Send(command);
            }

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
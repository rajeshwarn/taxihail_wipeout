#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
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
            var supportedLanguages = Enum.GetNames(typeof(SupportedLanguages));

            var displayQuestionLanguage = request.ClientLanguage ?? SupportedLanguages.en.ToString();

            //var res = allRatingTypes.Select(details => new
            //{
            //    Id = details.First().Id,
            //    Name = details.FirstOrDefault(t => t.Language == questionLanguage)
            //                  .SelectOrDefault(t => t.Name),
            //    RatingTypes = details
            //});

            // TODO: refactor that shit
            var res = allRatingTypes.Select(details => new RatingTypeWrapper
            {
                Id = details.First().Id,
                Name = details.FirstOrDefault(t => t.Language == displayQuestionLanguage)
                              .SelectOrDefault(t => t.Name),
                RatingTypes = supportedLanguages.Contains(request.ClientLanguage)
                    ? details.Select(s => new RatingType
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Language = s.Language,
                            IsHidden = s.IsHidden
                        }).Where(l => l.Language == request.ClientLanguage).ToArray()
                    : details.Select(s => new RatingType
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Language = s.Language,
                        IsHidden = s.IsHidden
                    }).ToArray()
            });

            return res;
        }

        public object Post(RatingTypesRequest request)
        {
            var ratingTypeId = Guid.NewGuid();
            var commands = new List<AddRatingType>();

            foreach (var ratingType in request.RatingTypes)
            {
                var existing = _dao.FindByName(ratingType.Name, ratingType.Language);
                if (existing != null)
                {
                    continue;
                }

                commands.Add(new AddRatingType
                {
                    RatingTypeId = ratingTypeId,
                    CompanyId = AppConstants.CompanyId,
                    Name = ratingType.Name,
                    Language = ratingType.Language
                });
            }

            _commandBus.Send(commands);

            return new
            {
                Id = ratingTypeId
            };
        }

        public object Put(RatingTypesRequest request)
        {
            var commands = new List<UpdateRatingType>();

            foreach (var ratingType in request.RatingTypes)
            {
                if (_dao.GetById(request.Id).Any(x => x.Id != request.Id && x.Name == ratingType.Name && x.Language == ratingType.Language))
                {
                    continue;
                }

                commands.Add(new UpdateRatingType
                {
                    RatingTypeId = request.Id,
                    CompanyId = AppConstants.CompanyId,
                    Name = ratingType.Name,
                    Language = ratingType.Language
                });
            }
            _commandBus.Send(commands);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }

        public object Delete(RatingTypesRequest request)
        {
            _commandBus.Send(new DeleteRatingType
            {
                CompanyId = AppConstants.CompanyId,
                RatingTypeId = request.Id,
                Languages = Enum.GetNames(typeof(SupportedLanguages))
            });

            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }
}
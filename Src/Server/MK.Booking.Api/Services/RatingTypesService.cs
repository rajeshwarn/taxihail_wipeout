﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class RatingTypesService : BaseApiService
    {
        private readonly IRatingTypeDao _dao;
        private readonly ICommandBus _commandBus;

        public RatingTypesService(IRatingTypeDao dao, ICommandBus commandBus)
        {
            _dao = dao;
            _commandBus = commandBus;
        }

        public RatingTypeWrapper[] Get(RatingTypesRequest request)
        {
            var allRatingTypes = _dao.GetAll();
            var supportedLanguages = Enum.GetNames(typeof(SupportedLanguages));
            var displayQuestionLanguage = request.ClientLanguage ?? SupportedLanguages.en.ToString();

            return allRatingTypes.Select(details =>
            {
                // Try to use the prefered display name first for the wrapper name
                var preferedDisplayName = 
                    details.FirstOrDefault(t => t.Language == displayQuestionLanguage)
                            .SelectOrDefault(t => t.Name);

                // If we can't, we'll display the first available rating question
                var firstAvailableDisplayName = 
                    details.FirstOrDefault(t => !t.Name.IsNullOrEmpty())
                           .SelectOrDefault(t => t.Name);

                var ratingTypes = details.Select(s => new RatingType
                {
                    Id = s.Id,
                    Name = s.Name,
                    Language = s.Language,
                    IsHidden = s.IsHidden
                });
                
                // Wrap each rating type groups so that it can be nicely displayed in the admin web portal
                return new RatingTypeWrapper
                {
                    Id = details.First().Id,
                    Name = preferedDisplayName.IsNullOrEmpty()
                        ? firstAvailableDisplayName
                        : preferedDisplayName,
                    RatingTypes = supportedLanguages.Contains(request.ClientLanguage)
                        ? ratingTypes.Where(l => l.Language == request.ClientLanguage).ToArray() // Filter by lauguage
                        : ratingTypes.ToArray() // Return all
                };
            })
            .ToArray();
        }

        public object Post(RatingTypesRequest request)
        {
            var ratingTypeId = Guid.NewGuid();
            var commands = new List<AddRatingType>();

            foreach (var ratingType in request.RatingTypes)
            {
                var existing = _dao.FindByName(ratingType.Name, ratingType.Language);
                if (existing != null && existing.Id == ratingTypeId)
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

        public void Put(RatingTypesRequest request)
        {
            var commands = new List<UpdateRatingType>();
            var currentRatingTypes = _dao.GetById(request.Id);

            foreach (var ratingType in request.RatingTypes)
            {
                if (currentRatingTypes.Any(x => x.Name == ratingType.Name && x.Language == ratingType.Language))
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
        }

        public void Delete(Guid ratingTypeId)
        {
            _commandBus.Send(new DeleteRatingType
            {
                CompanyId = AppConstants.CompanyId,
                RatingTypeId = ratingTypeId
            });
        }
    }
}
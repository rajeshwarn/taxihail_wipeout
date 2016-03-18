#region

using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class SaveAddressService : BaseApiService
    {
        private readonly ICommandBus _commandBus;

        public SaveAddressService(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public void Post(SaveAddress request)
        {
            Validate(request);

            var command = new AddFavoriteAddress();

            Mapper.Map(request, command);
            command.AccountId = Session.UserId;
            command.Address.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;
            _commandBus.Send(command);
        }

        private void Validate(SaveAddress request)
        {
            var hasFriendlyName = request.Address.FriendlyName.HasValueTrimmed();
            var hasFullAddress = request.Address.FullAddress.HasValueTrimmed();
            var latitudeInclusiveBetween = request.Address.Latitude >= -90d && request.Address.Latitude <= 90d;
            var longitudeInclusiveBetween = request.Address.Longitude >= -180d && request.Address.Longitude <= 180d;

            if (!hasFriendlyName)
            {
                throw GenerateException(HttpStatusCode.BadRequest, "NotEmpty", "FriendlyName is emtpy");
            }

            if (!hasFullAddress)
            {
                throw GenerateException(HttpStatusCode.BadRequest, "NotEmpty", "FullAddress is emtpy");
            }

            if (!latitudeInclusiveBetween)
            {
                throw GenerateException(HttpStatusCode.BadRequest, "InclusiveBetween", "Latitude must be between -90d and 90d. You entered " + request.Address.Latitude);
            }

            if (!longitudeInclusiveBetween)
            {
                throw GenerateException(HttpStatusCode.BadRequest, "InclusiveBetween", "Longitude must be between -180d and 180d. You entered " + request.Address.Longitude);
            }
        }

        public void Delete(Guid id)
        {
            var command = new RemoveFavoriteAddress
            {
                Id = Guid.NewGuid(),
                AddressId = id,
                AccountId = Session.UserId
            };

            _commandBus.Send(command);
        }

        public void Put(SaveAddress request)
        {
            Validate(request);

            var command = new UpdateFavoriteAddress();

            Mapper.Map(request, command);
            command.AccountId = Session.UserId;
            command.Address.Id = request.Id;

            _commandBus.Send(command);
        }
    }
}
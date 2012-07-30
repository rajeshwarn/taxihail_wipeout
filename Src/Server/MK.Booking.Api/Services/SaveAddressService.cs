using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.FluentValidation;
using ServiceStack.FluentValidation.Results;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Validation;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using RegisterAccount = apcurium.MK.Booking.Api.Contract.Requests.RegisterAccount;

namespace apcurium.MK.Booking.Api.Services
{
    public class SaveAddressService : RestServiceBase<SaveAddress> 
    {
        public IValidator<SaveAddress> Validator { get; set; }

        private readonly ICommandBus _commandBus;
        public SaveAddressService(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public override object OnPost(SaveAddress request)
        {
            var result = this.Validator.Validate(request);

            if (!result.IsValid)
            {
                throw result.ToException();
            }

            var command = new Commands.AddAddress();
            
            AutoMapper.Mapper.Map(request, command);
            command.AccountId = new Guid(this.GetSession().UserAuthId);

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public override object OnDelete(SaveAddress request)
        {
            var command = new Commands.RemoveAddress
            {
                Id = Guid.NewGuid(),
                AddressId = request.Id,
                AccountId = new Guid(this.GetSession().UserAuthId)
            };

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public override object OnPut(SaveAddress request)
        {
            var command = new Commands.UpdateAddress();

            AutoMapper.Mapper.Map(request, command);
            command.AccountId = new Guid(this.GetSession().UserAuthId);

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

    }
}

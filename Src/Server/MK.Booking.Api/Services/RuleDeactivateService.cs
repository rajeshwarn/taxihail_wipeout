#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class RuleDeactivateService : Service
    {
        private readonly ICommandBus _commandBus;

        public RuleDeactivateService(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }


        public object Post(RuleDeactivateRequest request)
        {
            var command = Mapper.Map<DeactivateRule>(request);

            _commandBus.Send(command);

            return new
            {
                Id = command.RuleId
            };
        }
    }
}
#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class RuleActivateService : Service
    {
        private readonly ICommandBus _commandBus;

        public RuleActivateService(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }


        public object Post(RuleActivateRequest request)
        {
            var command = Mapper.Map<ActivateRule>(request);

            _commandBus.Send(command);

            return new
            {
                Id = command.RuleId
            };
        }
    }
}
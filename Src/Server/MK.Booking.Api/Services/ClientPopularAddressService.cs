using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Services
{
    public class ClientPopularAddressService : RestServiceBase<ClientPopularAddress>
    {
        public IValidator<ClientPopularAddressService> Validator { get; set; }

        private readonly ICommandBus _commandBus;
        protected IPopularAddressDao Dao { get; set; }
        private IConfigurationManager _configManager;

        public ClientPopularAddressService(IPopularAddressDao dao, ICommandBus commandBus, IConfigurationManager configManager)
        {
            _commandBus = commandBus;
            _configManager = configManager;
            Dao = dao; 
        }

        public override object OnGet(ClientPopularAddress request)
        {
            return new ClientPopularAddressResponse(Dao.GetAll());
        }

    }
}
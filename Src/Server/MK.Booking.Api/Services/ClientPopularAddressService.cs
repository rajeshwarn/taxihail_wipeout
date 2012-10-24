using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;
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
            float range = float.Parse(_configManager.GetSetting("Geoloc.PopularAddress.Range"));
            const double R = 6378137;

            var addressesInRange = new List<KeyValuePair<Guid, double>>();
            Dao.GetAll().ToList().ForEach(c =>
                                            {
                                                var d =
                                                Math.Acos(Math.Sin(c.Latitude.ToRad()) * Math.Sin(request.Latitude.ToRad()) +
                                                Math.Cos(c.Latitude.ToRad()) * Math.Cos(request.Latitude.ToRad()) *
                                                Math.Cos(request.Longitude.ToRad() - c.Longitude.ToRad())) * R;
                                                if(d<= range)
                                                {
                                                    addressesInRange.Add(new KeyValuePair<Guid, double>(c.Id, d));
                                                }
                                            });
            if(addressesInRange.Any())
            {
                addressesInRange = addressesInRange.OrderBy(c => c.Value).ToList();

                return Dao.FindById(addressesInRange[0].Key);
            }
            else
            {
                return null;
            }
        }
    }
    public static class DoubleExtension
    {
        public static double ToRad(this double number)
        {
            return number*Math.PI/180;
        }
    }
}
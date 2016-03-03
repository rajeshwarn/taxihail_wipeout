#region

using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using AutoMapper;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ClientPopularAddressService : BaseApiService
    {

        public ClientPopularAddressService(IPopularAddressDao dao)
        {
           
            Dao = dao;
        }
        //TODO MKTAXI-3915: Handle this
        //public IValidator<ClientPopularAddress> Validator { get; set; }
        protected IPopularAddressDao Dao { get; set; }

        public object Get(ClientPopularAddress request)
        {
	        var addresses = Dao.GetAll().Select(Mapper.Map<Address>);


			return new ClientPopularAddressResponse(addresses);
        }

		public object Get(AdminPopularAddress request)
		{
			return new AdminPopularAddressResponse(Dao.GetAll());
		}
    }
}
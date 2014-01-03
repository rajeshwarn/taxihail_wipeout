#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ClientPopularAddressService : Service
    {

        public ClientPopularAddressService(IPopularAddressDao dao)
        {
           
            Dao = dao;
        }

        public IValidator<ClientPopularAddressService> Validator { get; set; }
        protected IPopularAddressDao Dao { get; set; }

        public object Get(ClientPopularAddress request)
        {
            return new ClientPopularAddressResponse(Dao.GetAll());
        }
    }
}
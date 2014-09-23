#region

using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class RatingTypesService : Service
    {
        private readonly IRatingTypeDao _dao;

        public RatingTypesService(IRatingTypeDao dao)
        {
            _dao = dao;
        }

        public object Get(RatingTypesRequest request)
        {
            var allRatingTypes = _dao.GetAll();
            return allRatingTypes.Where(r => r.Language == request.Language).ToList();
        }
    }
}
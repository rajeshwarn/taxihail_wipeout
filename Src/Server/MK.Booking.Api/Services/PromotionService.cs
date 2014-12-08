using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class PromotionService : Service
    {
        private readonly IPromotionDao _promotionDao;

        public PromotionService(IPromotionDao promotionDao)
        {
            _promotionDao = promotionDao;
        }

        public object Get(ActivePromotions request)
        {
            return _promotionDao.GetAllCurrentlyActive()
                .Select(x => new ActivePromotion
                    {
                        Name = x.Name,
                        Description = x.Description,
                        Code = x.Code,
                        ExpirationDate = x.GetEndDateTime()
                    })
                .ToArray();
        }
    }
}
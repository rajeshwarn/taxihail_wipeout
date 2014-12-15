using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Enumeration;
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
            var progress = _promotionDao.GetAllProgress();

            return _promotionDao.GetAllCurrentlyActive()
                .Select(x => new ActivePromotion
                    {
                        Name = x.Name,
                        Description = x.Description,
                        Code = x.Code,
                        ExpirationDate = x.GetEndDateTime(),
                        Progress = GetFormattedProgressString(x, progress.FirstOrDefault(p => p.PromoId == x.Id))
                    })
                .ToArray();
        }

        private string GetFormattedProgressString(PromotionDetail promotionDetail, PromotionProgressDetail progressDetail)
        {
            var formattedProgressString = string.Empty;

            if (promotionDetail.TriggerSettings.Type == PromotionTriggerTypes.RideCount.Id)
            {
                formattedProgressString = string.Format("{0}/{1}",
                    (progressDetail == null || progressDetail.RideCount == null) 
                    ? 0
                    : progressDetail.RideCount,
                    promotionDetail.TriggerSettings.RideCount);
            }
            else if (promotionDetail.TriggerSettings.Type == PromotionTriggerTypes.AmountSpent.Id)
            {
                formattedProgressString = string.Format("{0}/{1}",
                    (progressDetail == null || progressDetail.AmountSpent == null)
                    ? 0.0
                    : progressDetail.AmountSpent,
                    promotionDetail.TriggerSettings.AmountSpent);
            }

            return formattedProgressString;
        }
    }
}
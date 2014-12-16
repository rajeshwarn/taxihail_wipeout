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
                .Select(x =>
                {
                    var progressDetails = progress.FirstOrDefault(p => p.PromoId == x.Id);
                    return new ActivePromotion
                                 {
                                     Name = x.Name,
                                     Description = x.Description,
                                     Code = x.Code,
                                     ExpirationDate = x.GetEndDateTime(),
                                     Progress = GetProgress(x, progressDetails),
                                     UnlockGoal = GetUnlockGoal(x)
                                 };
                }).ToArray();
        }

        private double? GetProgress(PromotionDetail promotionDetail, PromotionProgressDetail progressDetail)
        {
            if (promotionDetail.TriggerSettings.Type == PromotionTriggerTypes.RideCount.Id)
            {
                return (progressDetail == null || progressDetail.RideCount == null)
                        ? 0
                        : progressDetail.RideCount;
            }
            else if (promotionDetail.TriggerSettings.Type == PromotionTriggerTypes.AmountSpent.Id)
            {
                return (progressDetail == null || progressDetail.AmountSpent == null)
                        ? 0.0
                        : progressDetail.AmountSpent;
            }

            return null;
        }

        private double? GetUnlockGoal(PromotionDetail promotionDetail)
        {
            if (promotionDetail.TriggerSettings.Type == PromotionTriggerTypes.RideCount.Id)
            {
                return promotionDetail.TriggerSettings.RideCount;
            }
            else if (promotionDetail.TriggerSettings.Type == PromotionTriggerTypes.AmountSpent.Id)
            {
                return promotionDetail.TriggerSettings.AmountSpent;
            }

            return null;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
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
            return _promotionDao.GetAllCurrentlyActive()
                .Select(promotionDetail =>
                {
                    var activePromotion = new ActivePromotion
                    {
                        Name = promotionDetail.Name,
                        Description = promotionDetail.Description,
                        Code = promotionDetail.Code,
                        ExpirationDate = promotionDetail.GetEndDateTime()
                    };

                    var accountId = new Guid(this.GetSession().UserAuthId);
                    AddProgressToPromotion(accountId, promotionDetail, activePromotion);

                    return activePromotion;
                }).ToArray();
        }

        private void AddProgressToPromotion(Guid accoundId, PromotionDetail promotionDetail, ActivePromotion activePromotion)
        {
            var progressDetail = _promotionDao.GetProgress(accoundId, promotionDetail.Id);
            activePromotion.Progress = GetProgress(promotionDetail, progressDetail);
            activePromotion.UnlockGoal = GetUnlockGoal(promotionDetail);
        }

        private double? GetProgress(PromotionDetail promotionDetail, PromotionProgressDetail progressDetail)
        {
            if (promotionDetail.TriggerSettings.Type == PromotionTriggerTypes.RideCount)
            {
                return (progressDetail == null || progressDetail.RideCount == null)
                        ? 0
                        : progressDetail.RideCount;
            }
            else if (promotionDetail.TriggerSettings.Type == PromotionTriggerTypes.AmountSpent)
            {
                return (progressDetail == null || progressDetail.AmountSpent == null)
                        ? 0.0
                        : progressDetail.AmountSpent;
            }

            return null;
        }

        private double? GetUnlockGoal(PromotionDetail promotionDetail)
        {
            if (promotionDetail.TriggerSettings.Type == PromotionTriggerTypes.RideCount)
            {
                return promotionDetail.TriggerSettings.RideCount;
            }
            else if (promotionDetail.TriggerSettings.Type == PromotionTriggerTypes.AmountSpent)
            {
                return promotionDetail.TriggerSettings.AmountSpent;
            }

            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IPromotionDao
    {
        IEnumerable<PromotionDetail> GetAll();

        IEnumerable<PromotionProgressDetail> GetProgressByPromo(Guid promoId);

        PromotionProgressDetail GetProgress(Guid accountId, Guid promoId);

        IEnumerable<PromotionDetail> GetUnlockedPromotionsForUser(Guid accountId);

        IEnumerable<PromotionDetail> GetAllCurrentlyActiveAndPublished(PromotionTriggerTypes? triggerType = null);

        IEnumerable<PromotionDetail> GetAllCurrentlyActive(PromotionTriggerTypes? triggerType = null);

        PromotionDetail FindById(Guid id);

        PromotionDetail FindByPromoCode(string promoCode);

        PromotionUsageDetail FindByOrderId(Guid orderId);

        IEnumerable<PromotionUsageDetail> FindPromotionUsageByAccountId(Guid accountid);

        IEnumerable<PromotionUsageDetail> GetRedeemedPromotionUsages(Guid promoId);
    }
}
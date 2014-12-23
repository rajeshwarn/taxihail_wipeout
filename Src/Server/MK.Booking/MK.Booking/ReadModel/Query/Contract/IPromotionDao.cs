using System;
using System.Collections.Generic;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IPromotionDao
    {
        IEnumerable<PromotionDetail> GetAll();

        IEnumerable<PromotionProgressDetail> GetAllProgress(Guid promoId);

        PromotionProgressDetail GetProgress(Guid accountId, Guid promoId);

        IEnumerable<PromotionDetail> GetAllCurrentlyActive(PromotionTriggerTypes? triggerType = null);

        PromotionDetail FindById(Guid id);

        PromotionDetail FindByPromoCode(string promoCode);

        PromotionUsageDetail FindByOrderId(Guid orderId);
        IEnumerable<PromotionUsageDetail> GetRedeemedPromotionUsages(Guid promoId);
    }
}
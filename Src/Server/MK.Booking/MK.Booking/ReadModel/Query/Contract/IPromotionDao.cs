using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IPromotionDao
    {
        IEnumerable<PromotionDetail> GetAll();

        IEnumerable<PromotionProgressDetail> GetAllProgress();

        IEnumerable<PromotionDetail> GetAllCurrentlyActive();

        PromotionDetail FindById(Guid id);

        PromotionDetail FindByPromoCode(string promoCode);

        PromotionUsageDetail FindByOrderId(Guid orderId);
        IEnumerable<PromotionUsageDetail> GetRedeemedPromotionUsages(Guid promoId);
    }
}
using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IPromotionDao
    {
        IEnumerable<PromotionDetail> GetAll();
        PromotionDetail FindById(Guid id);
        PromotionDetail FindByPromoCode(string promoCode);
        PromotionUsageDetail FindByOrderId(Guid orderId);
    }
}
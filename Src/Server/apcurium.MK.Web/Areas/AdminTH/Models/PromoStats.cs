using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
    public class PromoStats
    {
        public PromoStats(PromotionUsageDetail[] promotionUsageDetails)
        {
            UsersUsage = new Dictionary<string, int>();

            if (!promotionUsageDetails.Any())
            {
                return;
            }

            PromoCode = promotionUsageDetails[0].Code;
            UsageCount = promotionUsageDetails.Length;
            TotalUsageAmount = (double)promotionUsageDetails.Where(p => p.DateRedeemed.HasValue).Sum(x => x.AmountSaved);

            foreach (var promotionUsage in promotionUsageDetails)
            {
                if (promotionUsage.UserId == null)
                {
                    continue;
                }

                if (UsersUsage.ContainsKey(promotionUsage.UserId))
                {
                    UsersUsage[promotionUsage.UserId]++;
                }
                else
                {
                    UsersUsage.Add(promotionUsage.UserId, 1);
                }
            }
        }

        public string PromoCode { get; set; }

        public int UsageCount { get; set; }

        public double TotalUsageAmount { get; set; }

        public Dictionary<string, int> UsersUsage { get; set; }
    }
}
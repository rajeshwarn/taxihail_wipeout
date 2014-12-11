using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceStack.Text;

namespace apcurium.MK.Booking.ReadModel
{
    public class PromotionStatisticDetail
    {
        [Key]
        public Guid Id { get; set; }

        public string PromoCode { get; set; }

        public int UsageCount { get; set; }

        public double TotalUsageAmount { get; set; }

        public string SerializedUsersUsage { get; set; }

        [NotMapped]
        public Dictionary<string, int> UsersUsage
        {
            get { return SerializedUsersUsage.FromJson<Dictionary<string, int>>(); }
            set { SerializedUsersUsage = value.ToJson(); }
        } 
    }
}

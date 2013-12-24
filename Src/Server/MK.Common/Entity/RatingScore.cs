#region

using System;

#endregion

namespace apcurium.MK.Common.Entity
{
    public class RatingScore
    {
        public Guid RatingTypeId { get; set; }
        public int Score { get; set; }
        public string Name { get; set; }
    }
}
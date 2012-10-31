using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Common.Entity
{
    public class OrderRatings
    {
        public OrderRatings()
        {
            RatingScores = new List<RatingScore>();
        }

        public Guid OrderId { get; set; }
        public string Note { get; set; }
        public List<RatingScore> RatingScores { get; set; }
    }
}

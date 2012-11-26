using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Common.Entity
{
    public class RatingScore
    {
        public Guid RatingTypeId { get; set; }
        public int Score { get; set; }
        public string Name { get; set; }
    }
}

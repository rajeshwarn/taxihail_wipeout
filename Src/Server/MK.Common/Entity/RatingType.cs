using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Common.Entity
{
    public class RatingType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsHidden { get; set; }
    }
}

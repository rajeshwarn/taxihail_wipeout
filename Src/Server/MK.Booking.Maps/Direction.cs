﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Maps
{
    public class Direction
    {
        public int? Distance { get; set; }
        public double? Price { get; set; }

        public string FormattedPrice { get; set; }
        public string FormattedDistance { get; set; }
    }
}

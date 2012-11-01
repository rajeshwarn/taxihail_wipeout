﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Common.Entity
{
    public class Tariff
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public decimal FlatRate { get; set; }
        public double KilometricRate { get; set; }
        public double MarginOfError { get; set; }
        public decimal PassengerRate { get; set; }
        public int DaysOfTheWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Type { get; set; }
    }
}

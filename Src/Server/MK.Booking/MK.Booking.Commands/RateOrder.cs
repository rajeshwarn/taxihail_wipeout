#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class RateOrder : ICommand
    {
        public RateOrder()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }
        public string Note { get; set; }
        public List<RatingScore> RatingScores { get; set; }
        public Guid Id { get; set; }
    }
}
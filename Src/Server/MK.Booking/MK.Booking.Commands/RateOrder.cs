using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class RateOrder : ICommand
    {
        public RateOrder()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Note { get; set; }
        public List<RatingScore> RatingScores { get; set; }
    }
}

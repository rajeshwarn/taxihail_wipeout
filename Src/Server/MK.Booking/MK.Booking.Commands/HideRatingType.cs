using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class HideRatingType : ICommand
    {
        public HideRatingType()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid RatingTypeId { get; set; }
        public Guid CompanyId { get; set; }
    }
}
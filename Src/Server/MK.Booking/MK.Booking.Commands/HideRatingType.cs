#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class HideRatingType : ICommand
    {
        public HideRatingType()
        {
            Id = Guid.NewGuid();
        }

        public Guid RatingTypeId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid Id { get; set; }
    }
}
using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DeleteRatingType : ICommand
    {
        public DeleteRatingType()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public Guid CompanyId { get; set; }
        public Guid RatingTypeId { get; set; }
    }
}

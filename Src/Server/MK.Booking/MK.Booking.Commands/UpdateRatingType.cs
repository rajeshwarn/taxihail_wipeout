using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateRatingType : ICommand
    {
        public UpdateRatingType()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
        public Guid RatingTypeId { get; set; }
    }
}
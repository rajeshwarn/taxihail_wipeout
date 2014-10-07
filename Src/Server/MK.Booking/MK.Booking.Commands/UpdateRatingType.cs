#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class UpdateRatingType : ICommand
    {
        public UpdateRatingType()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
        public Guid RatingTypeId { get; set; }
        public string Language { get; set; }
    }
}
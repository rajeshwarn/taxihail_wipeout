#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class AddRatingType : ICommand
    {
        public AddRatingType()
        {
            Id = Guid.NewGuid();
        }

        public Guid RatingTypeId { get; set; }
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
        public Guid Id { get; set; }
        public string Language { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddRatingType : ICommand
    {
        public AddRatingType()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid RatingTypeId { get; set; }
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
    }
}

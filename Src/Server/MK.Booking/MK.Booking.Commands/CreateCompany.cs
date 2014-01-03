#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class CreateCompany : ICommand
    {
        public CreateCompany()
        {
            Id = Guid.NewGuid();
        }

        public Guid CompanyId { get; set; }
        public Guid Id { get; set; }
    }
}
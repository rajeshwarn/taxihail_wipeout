using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class ChangeOrderDispatchCompany : ICommand
    {
        public ChangeOrderDispatchCompany()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public Guid OrderId { get; set; }
        public string DispatchCompanyName { get; set; }
        public string DispatchCompanyKey { get; set; }
    }
}

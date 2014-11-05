using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class PrepareOrderForNextDispatch : ICommand
    {
        public PrepareOrderForNextDispatch()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public Guid OrderId { get; set; }
        public string DispatchCompanyName { get; set; }
        public string DispatchCompanyKey { get; set; }
    }
}

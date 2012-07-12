using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;


namespace apcurium.MK.Booking.Commands
{
    public class CancelOrder : ICommand
    {
        public CancelOrder()
        {
            Id = Guid.NewGuid();            
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }        
    }
}

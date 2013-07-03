using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Commands
{
    public class UpdatePaymentSettings : ICommand
    {
        public UpdatePaymentSettings()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get;  set; }
        
        public ServerPaymentSettings ServerPaymentSettings { get; set; }
    }
}

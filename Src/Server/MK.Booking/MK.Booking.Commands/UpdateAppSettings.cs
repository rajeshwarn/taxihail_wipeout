using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateAppSettings : ICommand
    {
        public UpdateAppSettings()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get;  set; }
        public string Key { get;  set; }
        public string Value { get;  set; }
    }
}

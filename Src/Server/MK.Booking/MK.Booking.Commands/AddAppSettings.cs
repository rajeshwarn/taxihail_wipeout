using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddAppSettings : ICommand
    {
        public AddAppSettings()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get;  private set; }
        public Guid CompanyId { get; set; }
        public string Key { get; set; }
        public string Value { get;  set; }

    }
}

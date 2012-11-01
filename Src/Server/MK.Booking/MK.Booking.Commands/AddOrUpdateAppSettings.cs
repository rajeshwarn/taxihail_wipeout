using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddOrUpdateAppSettings : ICommand
    {
        public AddOrUpdateAppSettings()
        {
            Id = Guid.NewGuid();
            AppSettings = new Dictionary<string, string>();
        }
        public Guid Id { get;  set; }
        //public string Key { get;  set; }
        //public string Value { get;  set; }
        public IDictionary<string, string> AppSettings { get; set; }
    }
}

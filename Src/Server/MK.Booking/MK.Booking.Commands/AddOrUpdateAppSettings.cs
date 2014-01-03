#region

using System;
using System.Collections.Generic;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class AddOrUpdateAppSettings : ICommand
    {
        public AddOrUpdateAppSettings()
        {
            Id = Guid.NewGuid();
            AppSettings = new Dictionary<string, string>();
        }

        //public string Key { get;  set; }
        //public string Value { get;  set; }
        public IDictionary<string, string> AppSettings { get; set; }

        public Guid CompanyId { get; set; }
        public Guid Id { get; set; }
    }
}
using System;
using System.Collections.Generic;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DeleteAppSettings : ICommand
    {
        public DeleteAppSettings()
        {
            Id = Guid.NewGuid();
            AppSettings = new Dictionary<string, string>();
        }

        public IDictionary<string, string> AppSettings { get; set; }

        public Guid CompanyId { get; set; }
        public Guid Id { get; set; }
    }
}

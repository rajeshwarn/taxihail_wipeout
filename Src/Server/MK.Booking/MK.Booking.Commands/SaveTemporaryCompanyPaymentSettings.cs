using System;
using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class SaveTemporaryCompanyPaymentSettings : ICommand
    {
        public SaveTemporaryCompanyPaymentSettings()
        {
            Id = Guid.NewGuid();
        }

        public ServerPaymentSettings ServerPaymentSettings { get; set; }

        public Guid Id { get; set; }
    }
}

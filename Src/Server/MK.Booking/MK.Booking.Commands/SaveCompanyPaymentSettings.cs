using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class SaveCompanyPaymentSettings : ICommand
    {
        public SaveCompanyPaymentSettings()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public string SerializedCompanyPaymentSettings { get; set; }
    }
}

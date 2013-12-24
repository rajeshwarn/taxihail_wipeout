#region

using System;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class UpdatePaymentSettings : ICommand
    {
        public UpdatePaymentSettings()
        {
            Id = Guid.NewGuid();
            CompanyId = AppConstants.CompanyId;
        }

        public ServerPaymentSettings ServerPaymentSettings { get; set; }

        public Guid CompanyId { get; set; }
        public Guid Id { get; set; }
    }
}
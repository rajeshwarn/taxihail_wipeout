using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IConfigurationDao
    {
        ServerPaymentSettings GetPaymentSettings();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class PaymentService : Service
    {
        private readonly IConfigurationDao _configurationDao;

        public PaymentService(IConfigurationDao configurationDao)
        {
            _configurationDao = configurationDao;
        }

        public PaymentSettingsResponse Get(PaymentSettingsRequest request)
        {
            return new PaymentSettingsResponse()
                {
                    ClientPaymentSettings = _configurationDao.GetPaymentSettings()
                };
        }

    }
}

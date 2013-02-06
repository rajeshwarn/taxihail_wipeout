using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IRuleDao
    {
        IList<RuleDetail> GetAll();

        RuleDetail GetActiveDisableRule( bool isFutureBooking, DateTime pickupDate);

        RuleDetail GetActiveWarningRule(bool isFutureBooking, DateTime pickupDate);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IRuleDao
    {
        IList<RuleDetail> GetAll();

        IList<RuleDetail> GetActiveDisableRule( bool isFutureBooking, string zone);

        IList<RuleDetail> GetActiveWarningRule(bool isFutureBooking, string zone);
    }
}

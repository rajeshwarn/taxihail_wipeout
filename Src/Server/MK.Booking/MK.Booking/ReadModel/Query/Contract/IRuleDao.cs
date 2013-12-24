using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IRuleDao
    {
        IList<RuleDetail> GetAll();

        IList<RuleDetail> GetActiveDisableRule(bool isFutureBooking);

        IList<RuleDetail> GetActiveWarningRule(bool isFutureBooking);
    }
}
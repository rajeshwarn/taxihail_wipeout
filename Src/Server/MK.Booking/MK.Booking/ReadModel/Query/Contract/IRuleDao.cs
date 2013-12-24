#region

using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IRuleDao
    {
        IList<RuleDetail> GetAll();

        IList<RuleDetail> GetActiveDisableRule(bool isFutureBooking);

        IList<RuleDetail> GetActiveWarningRule(bool isFutureBooking);
    }
}
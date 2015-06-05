using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IAccountChargeDao
    {
        IList<AccountChargeDetail> GetAll();
        AccountChargeDetail FindByAccountNumber(string number);

        IList<AccountQuestionAnswer> GetLastAnswersForAccountId(Guid accountId);
    }
}
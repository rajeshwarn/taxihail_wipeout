using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IAccountChargeDao
    {
        IList<AccountChargeDetail> GetAll();
        AccountChargeDetail FindByAccountNumber(string number);
    }
}
#region

using System;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IAccountDao
    {
        IList<AccountDetail> GetAll();
        AccountDetail FindById(Guid id);
        AccountDetail FindByEmail(string email);
        AccountDetail FindByFacebookId(string id);
        AccountDetail FindByTwitterId(string id);
        int? GetIbsAccountId(Guid accountId, string companyKey);
        string GetPayPalEncryptedRefreshToken(Guid id);

		AccountDetail[] FindByNamePattern(string name);

		AccountDetail[] FindByEmailPattern(string email);

		AccountDetail[] FindByPhoneNumberPattern(string phoneNumber);
    }
}
using apcurium.MK.Booking.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Comparer
{
	public class AccountDetailComparer : IEqualityComparer<AccountDetail>
	{
		public bool Equals(AccountDetail x, AccountDetail y)
		{
			//Check whether the compared objects reference the same data
			if (Object.ReferenceEquals(x, y))
			{
				return true;
			}

			//Check whether any of the compared objects is null
			if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
			{
				return false;
			}

			//Check GUID
			return x.Id.Equals(y.Id);
		}

		public int GetHashCode(AccountDetail obj)
		{
            // Check whether the object is null
            if (Object.ReferenceEquals(obj, null))
            {
                return 0;
            }

            int hashName = obj.Name == null ? 0 : obj.Name.GetHashCode();
			int hashEmail = obj.Email.GetHashCode();
			int hashPhone = obj.Settings.Phone.GetHashCode();

			int t = hashName ^ hashEmail ^ hashPhone;
			return t;
		}
	}
}

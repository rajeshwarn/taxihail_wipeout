using System.ComponentModel.DataAnnotations;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Contract.Validation
{
    public class RegisterAccountValidation : ValidationAttribute
    {
        public RegisterAccountValidation()
        {
            ErrorMessage = "NotNull";
        }

        public override bool IsValid(object value)
        {
            var registerAccount = value as RegisterAccount;

            if (registerAccount == null)
            {
                return false;
            }

            return registerAccount.Email.HasValueTrimmed() &&
                   registerAccount.Name.HasValueTrimmed() &&
                   registerAccount.Country != null &&
                   registerAccount.Country.Code.HasValueTrimmed();
        }
    }
}

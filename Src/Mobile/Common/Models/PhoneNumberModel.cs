using System;
using apcurium.MK.Common;
using apcurium.MK.Common.Helpers;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class PhoneNumberModel
    {
		CountryISOCode _country;

		string _phoneNumber;

        public CountryISOCode Country
        {
            get
            {
                return _country;
            }

            set
            {
                _country = value;
                PhoneNumberDatasourceChangedCallEvent();
            }
        }

        public string PhoneNumber
        {
            get
            {
                return _phoneNumber;
            }
            set
            {
                _phoneNumber = value;
                PhoneNumberDatasourceChangedCallEvent();
            }
        }

        public delegate void PhoneNumberDatasourceChangedEventHandler(object sender, PhoneNumberChangedEventArgs e);

        public event PhoneNumberDatasourceChangedEventHandler PhoneNumberDatasourceChanged;

        public void PhoneNumberDatasourceChangedCallEvent()
        {
			if (PhoneNumberDatasourceChanged != null)
			{
				PhoneNumberDatasourceChanged(this, new PhoneNumberChangedEventArgs() { Country = _country, PhoneNumber = _phoneNumber });
			}
        }

        public void NotifyChanges(object sender, PhoneNumberChangedEventArgs e)
        {
			_country = e.Country;

            if (e.PhoneNumber != null)
            {
                _phoneNumber = e.PhoneNumber;
            }
        }

        public bool IsNumberPossible()
        {
			return PhoneHelper.IsPossibleNumber(Country, PhoneNumber);
        }

        public string GetPhoneExample()
        {
            var countryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(Country));
            return countryCode.GetPhoneExample();
        }
    }

    public class PhoneNumberChangedEventArgs : EventArgs
    {
        public CountryISOCode Country { get; set; }

        public string PhoneNumber { get; set; }
    }
}
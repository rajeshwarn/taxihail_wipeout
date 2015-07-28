using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class PhoneNumberModel
    {
        CountryISOCode country;

        string phoneNumber;

        public CountryISOCode Country
        {
            get
            {
                return country;
            }

            set
            {
                country = value;
                PhoneNumberDatasourceChangedCallEvent();
            }
        }

        public string PhoneNumber
        {
            get
            {
                return phoneNumber;
            }
            set
            {
                phoneNumber = value;
                PhoneNumberDatasourceChangedCallEvent();
            }
        }

        public delegate void PhoneNumberDatasourceChangedEventHandler(object sender, PhoneNumberChangedEventArgs e);

        public event PhoneNumberDatasourceChangedEventHandler PhoneNumberDatasourceChanged;

        public void PhoneNumberDatasourceChangedCallEvent()
        {
            if (PhoneNumberDatasourceChanged != null)
                PhoneNumberDatasourceChanged(this, new PhoneNumberChangedEventArgs() { Country = country, PhoneNumber = phoneNumber });
        }

        public void NotifyChanges(object sender, PhoneNumberChangedEventArgs e)
        {
            this.country = e.Country;

            if (e.PhoneNumber != null)
            {
                this.phoneNumber = e.PhoneNumber;
            }
        }

        public bool IsNumberPossible()
        {
            CountryCode countryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(Country));
            return countryCode.IsNumberPossible(PhoneNumber);
        }

        public string GetPhoneExample()
        {
            CountryCode countryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(Country));
            return countryCode.GetPhoneExample();
        }
    }

    public class PhoneNumberChangedEventArgs : EventArgs
    {
        public CountryISOCode Country { get; set; }

        public string PhoneNumber { get; set; }
    }
}
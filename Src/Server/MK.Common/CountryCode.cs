using System;
using System.Text;
using System.Linq;

namespace apcurium.MK.Common
{
    /// <summary>
    /// http://www.iso.org/iso/home/standards/country_codes.htm
    /// http://www.itu.int/dms_pub/itu-t/opb/sp/T-SP-E.164C-2011-PDF-E.pdf
    /// </summary>
    public class CountryISOCode
    {
        public static readonly string[] countryISONames =
        {
            "Afghanistan","Åland Islands","Albania","Algeria","American Samoa","Andorra","Angola","Anguilla","Antarctica","Antigua and Barbuda","Argentina","Armenia","Aruba","Australia","Austria","Azerbaijan","Bahamas (the)","Bahrain","Bangladesh","Barbados","Belarus","Belgium","Belize","Benin","Bermuda","Bhutan","Bolivia (Plurinational State of)","Bonaire, Sint Eustatius and Saba","Bosnia and Herzegovina","Botswana","Bouvet Island","Brazil","British Indian Ocean Territory (the)","Brunei Darussalam","Bulgaria","Burkina Faso","Burundi","Cabo Verde","Cambodia","Cameroon","Canada","Cayman Islands (the)","Central African Republic (the)","Chad","Chile","China","Christmas Island","Cocos (Keeling) Islands (the)","Colombia","Comoros (the)","Congo (the Democratic Republic of the)","Congo (the)","Cook Islands (the)","Costa Rica","Côte d'Ivoire","Croatia","Cuba","Curaçao","Cyprus","Czech Republic (the)","Denmark","Djibouti","Dominica","Dominican Republic (the)","Ecuador","Egypt","El Salvador","Equatorial Guinea","Eritrea","Estonia","Ethiopia","Falkland Islands (the) [Malvinas]","Faroe Islands (the)","Fiji","Finland","France","French Guiana","French Polynesia","French Southern Territories (the)","Gabon","Gambia (the)","Georgia","Germany","Ghana","Gibraltar","Greece","Greenland","Grenada","Guadeloupe","Guam","Guatemala","Guernsey","Guinea","Guinea-Bissau","Guyana","Haiti","Heard Island and McDonald Islands","Holy See (the)","Honduras","Hong Kong","Hungary","Iceland","India","Indonesia","Iran (Islamic Republic of)","Iraq","Ireland","Isle of Man","Israel","Italy","Jamaica","Japan","Jersey","Jordan","Kazakhstan","Kenya","Kiribati","Korea (the Democratic People's Republic of)","Korea (the Republic of)","Kuwait","Kyrgyzstan","Lao People's Democratic Republic (the)","Latvia","Lebanon","Lesotho","Liberia","Libya","Liechtenstein","Lithuania","Luxembourg","Macao","Macedonia (the former Yugoslav Republic of)","Madagascar","Malawi","Malaysia","Maldives","Mali","Malta","Marshall Islands (the)","Martinique","Mauritania","Mauritius","Mayotte","Mexico","Micronesia (Federated States of)","Moldova (the Republic of)","Monaco","Mongolia","Montenegro","Montserrat","Morocco","Mozambique","Myanmar","Namibia","Nauru","Nepal","Netherlands (the)","Netherlands Antilles","Neutral Zone","New Caledonia","New Zealand","Nicaragua","Niger (the)","Nigeria","Niue","Norfolk Island","Northern Mariana Islands (the)","Norway","Oman","Pakistan","Palau","Palestine, State of","Panama","Papua New Guinea","Paraguay","Peru","Philippines (the)","Pitcairn","Poland","Portugal","Puerto Rico","Qatar","Réunion","Romania","Russian Federation (the)","Rwanda","Saint Barthélemy","Saint Helena, Ascension and Tristan da Cunha","Saint Kitts and Nevis","Saint Lucia","Saint Martin (French part)","Saint Pierre and Miquelon","Saint Vincent and the Grenadines","Samoa","San Marino","Sao Tome and Principe","Saudi Arabia","Senegal","Serbia","Seychelles","Sierra Leone","Singapore","Sint Maarten (Dutch part)","Slovakia","Slovenia","Solomon Islands","Somalia","South Africa","South Georgia and the South Sandwich Islands","South Sudan","Spain","Sri Lanka","Sudan (the)","Suriname","Svalbard and Jan Mayen","Swaziland","Sweden","Switzerland","Syrian Arab Republic","Taiwan (Province of China)","Tajikistan","Tanzania, United Republic of","Thailand","Timor-Leste","Togo","Tokelau","Tonga","Trinidad and Tobago","Tunisia","Turkey","Turkmenistan","Turks and Caicos Islands (the)","Tuvalu","Uganda","Ukraine","United Arab Emirates (the)","United Kingdom of Great Britain and Northern Ireland (the)","United States Minor Outlying Islands (the)","United States of America (the)","Uruguay","Uzbekistan","Vanuatu","Venezuela (Bolivarian Republic of)","Viet Nam","Virgin Islands (British)","Virgin Islands (U.S.)","Wallis and Futuna","Western Sahara*","Yemen","Zambia","Zimbabwe"
        };

        public static readonly string[] countryISOCodes =
        {
            "AF","AX","AL","DZ","AS","AD","AO","AI","AQ","AG","AR","AM","AW","AU","AT","AZ","BS","BH","BD","BB","BY","BE","BZ","BJ","BM","BT","BO","BQ","BA","BW","BV","BR","IO","BN","BG","BF","BI","CV","KH","CM","CA","KY","CF","TD","CL","CN","CX","CC","CO","KM","CD","CG","CK","CR","CI","HR","CU","CW","CY","CZ","DK","DJ","DM","DO","EC","EG","SV","GQ","ER","EE","ET","FK","FO","FJ","FI","FR","GF","PF","TF","GA","GM","GE","DE","GH","GI","GR","GL","GD","GP","GU","GT","GG","GN","GW","GY","HT","HM","VA","HN","HK","HU","IS","IN","ID","IR","IQ","IE","IM","IL","IT","JM","JP","JE","JO","KZ","KE","KI","KP","KR","KW","KG","LA","LV","LB","LS","LR","LY","LI","LT","LU","MO","MK","MG","MW","MY","MV","ML","MT","MH","MQ","MR","MU","YT","MX","FM","MD","MC","MN","ME","MS","MA","MZ","MM","NA","NR","NP","NL","AN","NT","NC","NZ","NI","NE","NG","NU","NF","MP","NO","OM","PK","PW","PS","PA","PG","PY","PE","PH","PN","PL","PT","PR","QA","RE","RO","RU","RW","BL","SH","KN","LC","MF","PM","VC","WS","SM","ST","SA","SN","RS","SC","SL","SG","SX","SK","SI","SB","SO","ZA","GS","SS","ES","LK","SD","SR","SJ","SZ","SE","CH","SY","TW","TJ","TZ","TH","TL","TG","TK","TO","TT","TN","TR","TM","TC","TV","UG","UA","AE","GB","UM","US","UY","UZ","VU","VE","VN","VG","VI","WF","EH","YE","ZM","ZW"
        };

        private string _code;

        public string Code
        {
            get
            {
                return _code;
            }

            set
            {
                if (value == null || countryISOCodes.Count(c => c == value) == 1)
                {
                    _code = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Country code should be according to http://www.iso.org/iso/home/standards/country_codes.htm and belongs to Officially assigned codes");
                }
            }
        }

        public CountryISOCode()
        {
        }

        public CountryISOCode(string code)
        {
            _code = null;
            Code = code;
        }
    }

    public struct CountryCode
    {
        public static readonly CountryCode[] CountryCodes;

        private int _countryDialCode;

        static CountryCode()
        {
            CountryCodes = new CountryCode[CountryISOCode.countryISOCodes.Length];

            for (int i = 0; i < CountryISOCode.countryISOCodes.Length; i++)
            {
                CountryCodes[i] = new CountryCode(CountryISOCode.countryISONames[i], CountryISOCode.countryISOCodes[i]);
            }
        }
        
        public string CountryName { get; set; }

        private CountryISOCode _countryIsoCode;
        public CountryISOCode CountryISOCode
        {
            get
            {
                return _countryIsoCode;
            }
            set
            {
                _countryIsoCode = value;
                _countryDialCode = libphonenumber.PhoneNumberUtil.Instance.GetCountryCodeForRegion(_countryIsoCode.Code);
            }
        }
        
        public int CountryDialCode
        {
            get
            {
                return _countryDialCode;
            }
        }

        public string СountryDialCodeInternationalFormat
        {
            get
            {
                if (CountryDialCode > 0)
                {
                    return "+" + CountryDialCode;
                }
                return null;
            }
        }

        public CountryCode(string countryName, string countryISOCode):this()
        {
            _countryIsoCode = new CountryISOCode();
            _countryDialCode = 0;
            CountryName = countryName;
            CountryISOCode = new CountryISOCode(countryISOCode);
        }

        public string GetTextCountryDialCode()
        {
            return CountryDialCode != 0 ? "+" + CountryDialCode : null;
        }

        public static int GetCountryCodeIndexByCountryISOCode(CountryISOCode countryISOCode)
        {
            return (countryISOCode != null ? GetCountryCodeIndexByCountryISOCode(countryISOCode.Code) : -1);
        }

        public static int GetCountryCodeIndexByCountryISOCode(string countryISOCode)
        {
            if (countryISOCode != null)
            {
                for (var i = 0; i < CountryCodes.Length; i++)
                {
                    if (countryISOCode == CountryCodes[i].CountryISOCode.Code)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static int GetCountryCodeIndexByCountryDialCode(int countryDialCode)
        {
           for (var i = 0; i < CountryCodes.Length; i++)
           {
              if (countryDialCode == CountryCodes[i].CountryDialCode)
              {
                  return i;
              }
            }

            return -1;
        }

		public static int GetCountryCodeIndexByCountryDialCode(string countryDialCode)
		{
			if (!string.IsNullOrEmpty(countryDialCode))
			{
				int dialCode;

				if (int.TryParse(countryDialCode.TrimStart ('+'), out dialCode))
				{
					return GetCountryCodeIndexByCountryDialCode(dialCode);
				}
			}

			return -1;
		}

		public static CountryCode GetCountryCodeByIndex(int index)
        {
            if (index >= 0 && index < CountryCodes.Length)
            {
                return CountryCodes[index];
            }

            return new CountryCode();
        }

        public string GetPhoneExample()
        {
            libphonenumber.PhoneNumber phoneNumberExample = libphonenumber.PhoneNumberUtil.Instance.GetExampleNumber(CountryISOCode.Code);
            string phoneNumberExampleText = phoneNumberExample.Format(libphonenumber.PhoneNumberUtil.PhoneNumberFormat.E164);
            return phoneNumberExampleText.Replace("+" + CountryDialCode, string.Empty);
        }

        public bool IsNumberPossible(string phoneNumber)
        {
            return libphonenumber.PhoneNumberUtil.Instance.IsPossibleNumber(phoneNumber, _countryIsoCode.Code);
        }

		public override string ToString()
		{
			return GetTextCountryDialCode();
		}
    }
}
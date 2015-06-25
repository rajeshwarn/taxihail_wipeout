using System;
using System.Text;
using System.Linq;

namespace apcurium.MK.Common
{
    /// <summary>
    /// http://www.iso.org/iso/home/standards/country_codes.htm
    /// http://www.itu.int/dms_pub/itu-t/opb/sp/T-SP-E.164C-2011-PDF-E.pdf
    /// </summary>
    public struct CountryCode
    {
        string countryISOCode;
        int countryDialCode;

        public string CountryName { get; set; }
        public string CountryISOCode
        {
            get
            {
                return countryISOCode;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || value.Length != 2)
                {
                    new ArgumentException("Country ISO code must be according to http://www.iso.org/iso/home/standards/country_codes.htm");
                }

                countryISOCode = value.ToUpper();
                countryDialCode = libphonenumber.PhoneNumberUtil.Instance.GetCountryCodeForRegion(countryISOCode);
            }
        }
        public int CountryDialCode
        {
            get
            {
                return countryDialCode;
            }
        }

        public CountryCode(string countryName, string countryISOCode):this()
        {
            this.countryISOCode = "";
            this.countryDialCode = 0;
            CountryName = countryName;
            CountryISOCode = countryISOCode;
        }

        public string GetTextCountryDialCode()
        {
            return CountryDialCode != 0 ? "+" + CountryDialCode.ToString() : null;
        }

        public static int GetCountryCodeIndexByCountryISOCode(string countryISOCode)
        {
            for (int i = 0; i < CountryCodes.Length; i++)
            {
                if (countryISOCode == CountryCodes[i].CountryISOCode)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int GetCountryCodeIndexByCountryDialCode(int countryDialCode)
        {
           for (int i = 0; i < CountryCodes.Length; i++)
           {
              if (countryDialCode == CountryCodes[i].CountryDialCode)
              {
                  return i;
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

        public readonly static CountryCode[] CountryCodes = new CountryCode[]
                {
                   new CountryCode("Afghanistan", "AF"),
                   new CountryCode("Åland Islands", "AX"),
                   new CountryCode("Albania", "AL"),
                   new CountryCode("Algeria", "DZ"),
                   new CountryCode("American Samoa", "AS"),
                   new CountryCode("Andorra", "AD"),
                   new CountryCode("Angola", "AO"),
                   new CountryCode("Anguilla", "AI"),
                   new CountryCode("Antarctica", "AQ"),
                   new CountryCode("Antigua and Barbuda", "AG"),
                   new CountryCode("Argentina", "AR"),
                   new CountryCode("Armenia", "AM"),
                   new CountryCode("Aruba", "AW"),
                   new CountryCode("Australia", "AU"),
                   new CountryCode("Austria", "AT"),
                   new CountryCode("Azerbaijan", "AZ"),
                   new CountryCode("Bahamas (the)", "BS"),
                   new CountryCode("Bahrain", "BH"),
                   new CountryCode("Bangladesh", "BD"),
                   new CountryCode("Barbados", "BB"),
                   new CountryCode("Belarus", "BY"),
                   new CountryCode("Belgium", "BE"),
                   new CountryCode("Belize", "BZ"),
                   new CountryCode("Benin", "BJ"),
                   new CountryCode("Bermuda", "BM"),
                   new CountryCode("Bhutan", "BT"),
                   new CountryCode("Bolivia (Plurinational State of)", "BO"),
                   new CountryCode("Bonaire, Sint Eustatius and Saba", "BQ"),
                   new CountryCode("Bosnia and Herzegovina", "BA"),
                   new CountryCode("Botswana", "BW"),
                   new CountryCode("Bouvet Island", "BV"),
                   new CountryCode("Brazil", "BR"),
                   new CountryCode("British Indian Ocean Territory (the)", "IO"),
                   new CountryCode("Brunei Darussalam", "BN"),
                   new CountryCode("Bulgaria", "BG"),
                   new CountryCode("Burkina Faso", "BF"),
                   new CountryCode("Burundi", "BI"),
                   new CountryCode("Cabo Verde", "CV"),
                   new CountryCode("Cambodia", "KH"),
                   new CountryCode("Cameroon", "CM"),
                   new CountryCode("Canada", "CA"),
                   new CountryCode("Cayman Islands (the)", "KY"),
                   new CountryCode("Central African Republic (the)", "CF"),
                   new CountryCode("Chad", "TD"),
                   new CountryCode("Chile", "CL"),
                   new CountryCode("China", "CN"),
                   new CountryCode("Christmas Island", "CX"),
                   new CountryCode("Cocos (Keeling) Islands (the)", "CC"),
                   new CountryCode("Colombia", "CO"),
                   new CountryCode("Comoros (the)", "KM"),
                   new CountryCode("Congo (the Democratic Republic of the)", "CD"),
                   new CountryCode("Congo (the)", "CG"),
                   new CountryCode("Cook Islands (the)", "CK"),
                   new CountryCode("Costa Rica", "CR"),
                   new CountryCode("Côte d'Ivoire", "CI"),
                   new CountryCode("Croatia", "HR"),
                   new CountryCode("Cuba", "CU"),
                   new CountryCode("Curaçao", "CW"),
                   new CountryCode("Cyprus", "CY"),
                   new CountryCode("Czech Republic (the)", "CZ"),
                   new CountryCode("Denmark", "DK"),
                   new CountryCode("Djibouti", "DJ"),
                   new CountryCode("Dominica", "DM"),
                   new CountryCode("Dominican Republic (the)", "DO"),
                   new CountryCode("Ecuador", "EC"),
                   new CountryCode("Egypt", "EG"),
                   new CountryCode("El Salvador", "SV"),
                   new CountryCode("Equatorial Guinea", "GQ"),
                   new CountryCode("Eritrea", "ER"),
                   new CountryCode("Estonia", "EE"),
                   new CountryCode("Ethiopia", "ET"),
                   new CountryCode("Falkland Islands (the) [Malvinas]", "FK"),
                   new CountryCode("Faroe Islands (the)", "FO"),
                   new CountryCode("Fiji", "FJ"),
                   new CountryCode("Finland", "FI"),
                   new CountryCode("France", "FR"),
                   new CountryCode("French Guiana", "GF"),
                   new CountryCode("French Polynesia", "PF"),
                   new CountryCode("French Southern Territories (the)", "TF"),
                   new CountryCode("Gabon", "GA"),
                   new CountryCode("Gambia (the)", "GM"),
                   new CountryCode("Georgia", "GE"),
                   new CountryCode("Germany", "DE"),
                   new CountryCode("Ghana", "GH"),
                   new CountryCode("Gibraltar", "GI"),
                   new CountryCode("Greece", "GR"),
                   new CountryCode("Greenland", "GL"),
                   new CountryCode("Grenada", "GD"),
                   new CountryCode("Guadeloupe", "GP"),
                   new CountryCode("Guam", "GU"),
                   new CountryCode("Guatemala", "GT"),
                   new CountryCode("Guernsey", "GG"),
                   new CountryCode("Guinea", "GN"),
                   new CountryCode("Guinea-Bissau", "GW"),
                   new CountryCode("Guyana", "GY"),
                   new CountryCode("Haiti", "HT"),
                   new CountryCode("Heard Island and McDonald Islands", "HM"),
                   new CountryCode("Holy See (the)", "VA"),
                   new CountryCode("Honduras", "HN"),
                   new CountryCode("Hong Kong", "HK"),
                   new CountryCode("Hungary", "HU"),
                   new CountryCode("Iceland", "IS"),
                   new CountryCode("India", "IN"),
                   new CountryCode("Indonesia", "ID"),
                   new CountryCode("Iran (Islamic Republic of)", "IR"),
                   new CountryCode("Iraq", "IQ"),
                   new CountryCode("Ireland", "IE"),
                   new CountryCode("Isle of Man", "IM"),
                   new CountryCode("Israel", "IL"),
                   new CountryCode("Italy", "IT"),
                   new CountryCode("Jamaica", "JM"),
                   new CountryCode("Japan", "JP"),
                   new CountryCode("Jersey", "JE"),
                   new CountryCode("Jordan", "JO"),
                   new CountryCode("Kazakhstan", "KZ"),
                   new CountryCode("Kenya", "KE"),
                   new CountryCode("Kiribati", "KI"),
                   new CountryCode("Korea (the Democratic People's Republic of)", "KP"),
                   new CountryCode("Korea (the Republic of)", "KR"),
                   new CountryCode("Kuwait", "KW"),
                   new CountryCode("Kyrgyzstan", "KG"),
                   new CountryCode("Lao People's Democratic Republic (the)", "LA"),
                   new CountryCode("Latvia", "LV"),
                   new CountryCode("Lebanon", "LB"),
                   new CountryCode("Lesotho", "LS"),
                   new CountryCode("Liberia", "LR"),
                   new CountryCode("Libya", "LY"),
                   new CountryCode("Liechtenstein", "LI"),
                   new CountryCode("Lithuania", "LT"),
                   new CountryCode("Luxembourg", "LU"),
                   new CountryCode("Macao", "MO"),
                   new CountryCode("Macedonia (the former Yugoslav Republic of)", "MK"),
                   new CountryCode("Madagascar", "MG"),
                   new CountryCode("Malawi", "MW"),
                   new CountryCode("Malaysia", "MY"),
                   new CountryCode("Maldives", "MV"),
                   new CountryCode("Mali", "ML"),
                   new CountryCode("Malta", "MT"),
                   new CountryCode("Marshall Islands (the)", "MH"),
                   new CountryCode("Martinique", "MQ"),
                   new CountryCode("Mauritania", "MR"),
                   new CountryCode("Mauritius", "MU"),
                   new CountryCode("Mayotte", "YT"),
                   new CountryCode("Mexico", "MX"),
                   new CountryCode("Micronesia (Federated States of)", "FM"),
                   new CountryCode("Moldova (the Republic of)", "MD"),
                   new CountryCode("Monaco", "MC"),
                   new CountryCode("Mongolia", "MN"),
                   new CountryCode("Montenegro", "ME"),
                   new CountryCode("Montserrat", "MS"),
                   new CountryCode("Morocco", "MA"),
                   new CountryCode("Mozambique", "MZ"),
                   new CountryCode("Myanmar", "MM"),
                   new CountryCode("Namibia", "NA"),
                   new CountryCode("Nauru", "NR"),
                   new CountryCode("Nepal", "NP"),
                   new CountryCode("Netherlands (the)", "NL"),
                   new CountryCode("Netherlands Antilles", "AN"),
                   new CountryCode("Neutral Zone", "NT"),
                   new CountryCode("New Caledonia", "NC"),
                   new CountryCode("New Zealand", "NZ"),
                   new CountryCode("Nicaragua", "NI"),
                   new CountryCode("Niger (the)", "NE"),
                   new CountryCode("Nigeria", "NG"),
                   new CountryCode("Niue", "NU"),
                   new CountryCode("Norfolk Island", "NF"),
                   new CountryCode("Northern Mariana Islands (the)", "MP"),
                   new CountryCode("Norway", "NO"),
                   new CountryCode("Oman", "OM"),
                   new CountryCode("Pakistan", "PK"),
                   new CountryCode("Palau", "PW"),
                   new CountryCode("Palestine, State of", "PS"),
                   new CountryCode("Panama", "PA"),
                   new CountryCode("Papua New Guinea", "PG"),
                   new CountryCode("Paraguay", "PY"),
                   new CountryCode("Peru", "PE"),
                   new CountryCode("Philippines (the)", "PH"),
                   new CountryCode("Pitcairn", "PN"),
                   new CountryCode("Poland", "PL"),
                   new CountryCode("Portugal", "PT"),
                   new CountryCode("Puerto Rico", "PR"),
                   new CountryCode("Qatar", "QA"),
                   new CountryCode("Réunion", "RE"),
                   new CountryCode("Romania", "RO"),
                   new CountryCode("Russian Federation (the)", "RU"),
                   new CountryCode("Rwanda", "RW"),
                   new CountryCode("Saint Barthélemy", "BL"),
                   new CountryCode("Saint Helena, Ascension and Tristan da Cunha", "SH"),
                   new CountryCode("Saint Kitts and Nevis", "KN"),
                   new CountryCode("Saint Lucia", "LC"),
                   new CountryCode("Saint Martin (French part)", "MF"),
                   new CountryCode("Saint Pierre and Miquelon", "PM"),
                   new CountryCode("Saint Vincent and the Grenadines", "VC"),
                   new CountryCode("Samoa", "WS"),
                   new CountryCode("San Marino", "SM"),
                   new CountryCode("Sao Tome and Principe", "ST"),
                   new CountryCode("Saudi Arabia", "SA"),
                   new CountryCode("Senegal", "SN"),
                   new CountryCode("Serbia", "RS"),
                   new CountryCode("Seychelles", "SC"),
                   new CountryCode("Sierra Leone", "SL"),
                   new CountryCode("Singapore", "SG"),
                   new CountryCode("Sint Maarten (Dutch part)", "SX"),
                   new CountryCode("Slovakia", "SK"),
                   new CountryCode("Slovenia", "SI"),
                   new CountryCode("Solomon Islands", "SB"),
                   new CountryCode("Somalia", "SO"),
                   new CountryCode("South Africa", "ZA"),
                   new CountryCode("South Georgia and the South Sandwich Islands", "GS"),
                   new CountryCode("South Sudan", "SS"),
                   new CountryCode("Spain", "ES"),
                   new CountryCode("Sri Lanka", "LK"),
                   new CountryCode("Sudan (the)", "SD"),
                   new CountryCode("Suriname", "SR"),
                   new CountryCode("Svalbard and Jan Mayen", "SJ"),
                   new CountryCode("Swaziland", "SZ"),
                   new CountryCode("Sweden", "SE"),
                   new CountryCode("Switzerland", "CH"),
                   new CountryCode("Syrian Arab Republic", "SY"),
                   new CountryCode("Taiwan (Province of China)", "TW"),
                   new CountryCode("Tajikistan", "TJ"),
                   new CountryCode("Tanzania, United Republic of", "TZ"),
                   new CountryCode("Thailand", "TH"),
                   new CountryCode("Timor-Leste", "TL"),
                   new CountryCode("Togo", "TG"),
                   new CountryCode("Tokelau", "TK"),
                   new CountryCode("Tonga", "TO"),
                   new CountryCode("Trinidad and Tobago", "TT"),
                   new CountryCode("Tunisia", "TN"),
                   new CountryCode("Turkey", "TR"),
                   new CountryCode("Turkmenistan", "TM"),
                   new CountryCode("Turks and Caicos Islands (the)", "TC"),
                   new CountryCode("Tuvalu", "TV"),
                   new CountryCode("Uganda", "UG"),
                   new CountryCode("Ukraine", "UA"),
                   new CountryCode("United Arab Emirates (the)", "AE"),
                   new CountryCode("United Kingdom of Great Britain and Northern Ireland (the)", "GB"),
                   new CountryCode("United States Minor Outlying Islands (the)", "UM"),
                   new CountryCode("United States of America (the)", "US"),
                   new CountryCode("Uruguay", "UY"),
                   new CountryCode("Uzbekistan", "UZ"),
                   new CountryCode("Vanuatu", "VU"),
                   new CountryCode("Venezuela (Bolivarian Republic of)", "VE"),
                   new CountryCode("Viet Nam", "VN"),
                   new CountryCode("Virgin Islands (British)", "VG"),
                   new CountryCode("Virgin Islands (U.S.)", "VI"),
                   new CountryCode("Wallis and Futuna", "WF"),
                   new CountryCode("Western Sahara*", "EH"),
                   new CountryCode("Yemen", "YE"),
                   new CountryCode("Zambia", "ZM"),
                   new CountryCode("Zimbabwe", "ZW"),
                };
    }
}
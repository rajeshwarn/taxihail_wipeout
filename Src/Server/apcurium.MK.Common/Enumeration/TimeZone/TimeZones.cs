using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Enumeration.TimeZone
{
    public static class TimeZoneHelper
    {
        private static TimeZoneInfo GetTimeZoneInfo(TimeZones value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var timeZoneId = fieldInfo.GetDisplayShortName();
            if (timeZoneId == null)
            {
                return null;
            }

            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }

        public static DateTime TransformToLocalTime(TimeZones timeZoneEnum, DateTime utc)
        {
            var timeZone = GetTimeZoneInfo(timeZoneEnum);
            if (timeZone == null)
            {
                return utc;
            }

            return utc + timeZone.GetUtcOffset(utc);
        }
    }

    public enum TimeZones
    {
        [Display(Name = "NOT SET (will show dates as UTC)")]
        NotSet,
        [Display(Name = "(UTC-12:00) International Date Line West", ShortName = "Dateline Standard Time")]
        DatelineStandardTime,
        [Display(Name = "(UTC-11:00) Coordinated Universal Time-11", ShortName = "UTC-11")]
        UTCMinus11,
        [Display(Name = "(UTC-10:00) Hawaii", ShortName = "Hawaiian Standard Time")]
        HawaiianStandardTime,
        [Display(Name = "(UTC-09:00) Alaska", ShortName = "Alaskan Standard Time")]
        AlaskanStandardTime,
        [Display(Name = "(UTC-08:00) Baja California", ShortName = "Pacific Standard Time (Mexico)")]
        PacificStandardTimeMexico,
        [Display(Name = "(UTC-08:00) Pacific Time (US & Canada)", ShortName = "Pacific Standard Time")]
        PacificStandardTime,
        [Display(Name = "(UTC-07:00) Arizona", ShortName = "US Mountain Standard Time")]
        USMountainStandardTime,
        [Display(Name = "(UTC-07:00) Chihuahua, La Paz, Mazatlan", ShortName = "Mountain Standard Time (Mexico)")]
        MountainStandardTimeMexico,
        [Display(Name = "(UTC-07:00) Mountain Time (US & Canada)", ShortName = "Mountain Standard Time")]
        MountainStandardTime,
        [Display(Name = "(UTC-06:00) Central America", ShortName = "Central America Standard Time")]
        CentralAmericaStandardTime,
        [Display(Name = "(UTC-06:00) Central Time (US & Canada)", ShortName = "Central Standard Time")]
        CentralStandardTime,
        [Display(Name = "(UTC-06:00) Guadalajara, Mexico City, Monterrey", ShortName = "Central Standard Time (Mexico)")]
        CentralStandardTimeMexico,
        [Display(Name = "(UTC-06:00) Saskatchewan", ShortName = "Canada Central Standard Time")]
        CanadaCentralStandardTime,
        [Display(Name = "(UTC-05:00) Bogota, Lima, Quito, Rio Branco", ShortName = "SA Pacific Standard Time")]
        SAPacificStandardTime,
        [Display(Name = "(UTC-05:00) Eastern Time (US & Canada)", ShortName = "Eastern Standard Time")]
        EasternStandardTime,
        [Display(Name = "(UTC-05:00) Indiana (East)", ShortName = "US Eastern Standard Time")]
        USEasternStandardTime,
        [Display(Name = "(UTC-04:30) Caracas", ShortName = "Venezuela Standard Time")]
        VenezuelaStandardTime,
        [Display(Name = "(UTC-04:00) Asuncion", ShortName = "Paraguay Standard Time")]
        ParaguayStandardTime,
        [Display(Name = "(UTC-04:00) Atlantic Time (Canada)", ShortName = "Atlantic Standard Time")]
        AtlanticStandardTime,
        [Display(Name = "(UTC-04:00) Cuiaba", ShortName = "Central Brazilian Standard Time")]
        CentralBrazilianStandardTime,
        [Display(Name = "(UTC-04:00) Georgetown, La Paz, Manaus, San Juan", ShortName = "SA Western Standard Time")]
        SAWesternStandardTime,
        [Display(Name = "(UTC-04:00) Santiago", ShortName = "Pacific SA Standard Time")]
        PacificSAStandardTime,
        [Display(Name = "(UTC-03:30) Newfoundland", ShortName = "Newfoundland Standard Time")]
        NewfoundlandStandardTime,
        [Display(Name = "(UTC-03:00) Brasilia", ShortName = "E. South America Standard Time")]
        ESouthAmericaStandardTime,
        [Display(Name = "(UTC-03:00) Buenos Aires", ShortName = "Argentina Standard Time")]
        ArgentinaStandardTime,
        [Display(Name = "(UTC-03:00) Cayenne, Fortaleza", ShortName = "SA Eastern Standard Time")]
        SAEasternStandardTime,
        [Display(Name = "(UTC-03:00) Greenland", ShortName = "Greenland Standard Time")]
        GreenlandStandardTime,
        [Display(Name = "(UTC-03:00) Montevideo", ShortName = "Montevideo Standard Time")]
        MontevideoStandardTime,
        [Display(Name = "(UTC-03:00) Salvador", ShortName = "Bahia Standard Time")]
        BahiaStandardTime,
        [Display(Name = "(UTC-02:00) Coordinated Universal Time-02", ShortName = "UTC-02")]
        UTCMinus02,
        [Display(Name = "(UTC-02:00) Mid-Atlantic - Old", ShortName = "Mid-Atlantic Standard Time")]
        MidAtlanticStandardTime,
        [Display(Name = "(UTC-01:00) Azores", ShortName = "Azores Standard Time")]
        AzoresStandardTime,
        [Display(Name = "(UTC-01:00) Cabo Verde Is.", ShortName = "Cape Verde Standard Time")]
        CapeVerdeStandardTime,
        [Display(Name = "(UTC) Casablanca", ShortName = "Morocco Standard Time")]
        MoroccoStandardTime,
        [Display(Name = "(UTC) Coordinated Universal Time", ShortName = "UTC")]
        UTC,
        [Display(Name = "(UTC) Dublin, Edinburgh, Lisbon, London", ShortName = "GMT Standard Time")]
        GMTStandardTime,
        [Display(Name = "(UTC) Monrovia, Reykjavik", ShortName = "Greenwich Standard Time")]
        GreenwichStandardTime,
        [Display(Name = "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna", ShortName = "W. Europe Standard Time")]
        WEuropeStandardTime,
        [Display(Name = "(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague", ShortName = "Central Europe Standard Time")]
        CentralEuropeStandardTime,
        [Display(Name = "(UTC+01:00) Brussels, Copenhagen, Madrid, Paris", ShortName = "Romance Standard Time")]
        RomanceStandardTime,
        [Display(Name = "(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb", ShortName = "Central European Standard Time")]
        CentralEuropeanStandardTime,
        [Display(Name = "(UTC+01:00) West Central Africa", ShortName = "W. Central Africa Standard Time")]
        WCentralAfricaStandardTime,
        [Display(Name = "(UTC+01:00) Windhoek", ShortName = "Namibia Standard Time")]
        NamibiaStandardTime,
        [Display(Name = "(UTC+02:00) Amman", ShortName = "Jordan Standard Time")]
        JordanStandardTime,
        [Display(Name = "(UTC+02:00) Athens, Bucharest", ShortName = "GTB Standard Time")]
        GTBStandardTime,
        [Display(Name = "(UTC+02:00) Beirut", ShortName = "Middle East Standard Time")]
        MiddleEastStandardTime,
        [Display(Name = "(UTC+02:00) Cairo", ShortName = "Egypt Standard Time")]
        EgyptStandardTime,
        [Display(Name = "(UTC+02:00) Damascus", ShortName = "Syria Standard Time")]
        SyriaStandardTime,
        [Display(Name = "(UTC+02:00) E. Europe", ShortName = "E. Europe Standard Time")]
        EEuropeStandardTime,
        [Display(Name = "(UTC+02:00) Harare, Pretoria", ShortName = "South Africa Standard Time")]
        SouthAfricaStandardTime,
        [Display(Name = "(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius", ShortName = "FLE Standard Time")]
        FLEStandardTime,
        [Display(Name = "(UTC+02:00) Istanbul", ShortName = "Turkey Standard Time")]
        TurkeyStandardTime,
        [Display(Name = "(UTC+02:00) Jerusalem", ShortName = "Israel Standard Time")]
        IsraelStandardTime,
        [Display(Name = "(UTC+02:00) Kaliningrad (RTZ 1)", ShortName = "Kaliningrad Standard Time")]
        KaliningradStandardTime,
        [Display(Name = "(UTC+02:00) Tripoli", ShortName = "Libya Standard Time")]
        LibyaStandardTime,
        [Display(Name = "(UTC+03:00) Baghdad", ShortName = "Arabic Standard Time")]
        ArabicStandardTime,
        [Display(Name = "(UTC+03:00) Kuwait, Riyadh", ShortName = "Arab Standard Time")]
        ArabStandardTime,
        [Display(Name = "(UTC+03:00) Minsk", ShortName = "Belarus Standard Time")]
        BelarusStandardTime,
        [Display(Name = "(UTC+03:00) Moscow, St. Petersburg, Volgograd (RTZ 2)", ShortName = "Russian Standard Time")]
        RussianStandardTime,
        [Display(Name = "(UTC+03:00) Nairobi", ShortName = "E. Africa Standard Time")]
        EAfricaStandardTime,
        [Display(Name = "(UTC+03:30) Tehran", ShortName = "Iran Standard Time")]
        IranStandardTime,
        [Display(Name = "(UTC+04:00) Abu Dhabi, Muscat", ShortName = "Arabian Standard Time")]
        ArabianStandardTime,
        [Display(Name = "(UTC+04:00) Baku", ShortName = "Azerbaijan Standard Time")]
        AzerbaijanStandardTime,
        [Display(Name = "(UTC+04:00) Izhevsk, Samara (RTZ 3)", ShortName = "Russia Time Zone 3")]
        RussiaTimeZone3,
        [Display(Name = "(UTC+04:00) Port Louis", ShortName = "Mauritius Standard Time")]
        MauritiusStandardTime,
        [Display(Name = "(UTC+04:00) Tbilisi", ShortName = "Georgian Standard Time")]
        GeorgianStandardTime,
        [Display(Name = "(UTC+04:00) Yerevan", ShortName = "Caucasus Standard Time")]
        CaucasusStandardTime,
        [Display(Name = "(UTC+04:30) Kabul", ShortName = "Afghanistan Standard Time")]
        AfghanistanStandardTime,
        [Display(Name = "(UTC+05:00) Ashgabat, Tashkent", ShortName = "West Asia Standard Time")]
        WestAsiaStandardTime,
        [Display(Name = "(UTC+05:00) Ekaterinburg (RTZ 4)", ShortName = "Ekaterinburg Standard Time")]
        EkaterinburgStandardTime,
        [Display(Name = "(UTC+05:00) Islamabad, Karachi", ShortName = "Pakistan Standard Time")]
        PakistanStandardTime,
        [Display(Name = "(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi", ShortName = "India Standard Time")]
        IndiaStandardTime,
        [Display(Name = "(UTC+05:30) Sri Jayawardenepura", ShortName = "Sri Lanka Standard Time")]
        SriLankaStandardTime,
        [Display(Name = "(UTC+05:45) Kathmandu", ShortName = "Nepal Standard Time")]
        NepalStandardTime,
        [Display(Name = "(UTC+06:00) Astana", ShortName = "Central Asia Standard Time")]
        CentralAsiaStandardTime,
        [Display(Name = "(UTC+06:00) Dhaka", ShortName = "Bangladesh Standard Time")]
        BangladeshStandardTime,
        [Display(Name = "(UTC+06:00) Novosibirsk (RTZ 5)", ShortName = "N. Central Asia Standard Time")]
        NCentralAsiaStandardTime,
        [Display(Name = "(UTC+06:30) Yangon (Rangoon)", ShortName = "Myanmar Standard Time")]
        MyanmarStandardTime,
        [Display(Name = "(UTC+07:00) Bangkok, Hanoi, Jakarta", ShortName = "SE Asia Standard Time")]
        SEAsiaStandardTime,
        [Display(Name = "(UTC+07:00) Krasnoyarsk (RTZ 6)", ShortName = "North Asia Standard Time")]
        NorthAsiaStandardTime,
        [Display(Name = "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi", ShortName = "China Standard Time")]
        ChinaStandardTime,
        [Display(Name = "(UTC+08:00) Irkutsk (RTZ 7)", ShortName = "North Asia East Standard Time")]
        NorthAsiaEastStandardTime,
        [Display(Name = "(UTC+08:00) Kuala Lumpur, Singapore", ShortName = "Singapore Standard Time")]
        SingaporeStandardTime,
        [Display(Name = "(UTC+08:00) Perth", ShortName = "W. Australia Standard Time")]
        WAustraliaStandardTime,
        [Display(Name = "(UTC+08:00) Taipei", ShortName = "Taipei Standard Time")]
        TaipeiStandardTime,
        [Display(Name = "(UTC+08:00) Ulaanbaatar", ShortName = "Ulaanbaatar Standard Time")]
        UlaanbaatarStandardTime,
        [Display(Name = "(UTC+09:00) Osaka, Sapporo, Tokyo", ShortName = "Tokyo Standard Time")]
        TokyoStandardTime,
        [Display(Name = "(UTC+09:00) Seoul", ShortName = "Korea Standard Time")]
        KoreaStandardTime,
        [Display(Name = "(UTC+09:00) Yakutsk (RTZ 8)", ShortName = "Yakutsk Standard Time")]
        YakutskStandardTime,
        [Display(Name = "(UTC+09:30) Adelaide", ShortName = "Cen. Australia Standard Time")]
        CenAustraliaStandardTime,
        [Display(Name = "(UTC+09:30) Darwin", ShortName = "AUS Central Standard Time")]
        AUSCentralStandardTime,
        [Display(Name = "(UTC+10:00) Brisbane", ShortName = "E. Australia Standard Time")]
        EAustraliaStandardTime,
        [Display(Name = "(UTC+10:00) Canberra, Melbourne, Sydney", ShortName = "AUS Eastern Standard Time")]
        AUSEasternStandardTime,
        [Display(Name = "(UTC+10:00) Guam, Port Moresby", ShortName = "West Pacific Standard Time")]
        WestPacificStandardTime,
        [Display(Name = "(UTC+10:00) Hobart", ShortName = "Tasmania Standard Time")]
        TasmaniaStandardTime,
        [Display(Name = "(UTC+10:00) Magadan", ShortName = "Magadan Standard Time")]
        MagadanStandardTime,
        [Display(Name = "(UTC+10:00) Vladivostok, Magadan (RTZ 9)", ShortName = "Vladivostok Standard Time")]
        VladivostokStandardTime,
        [Display(Name = "(UTC+11:00) Chokurdakh (RTZ 10)", ShortName = "Russia Time Zone 10")]
        RussiaTimeZone10,
        [Display(Name = "(UTC+11:00) Solomon Is., New Caledonia", ShortName = "Central Pacific Standard Time")]
        CentralPacificStandardTime,
        [Display(Name = "(UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky (RTZ 11)", ShortName = "Russia Time Zone 11")]
        RussiaTimeZone11,
        [Display(Name = "(UTC+12:00) Auckland, Wellington", ShortName = "New Zealand Standard Time")]
        NewZealandStandardTime,
        [Display(Name = "(UTC+12:00) Coordinated Universal Time+12", ShortName = "UTC+12")]
        UTCPlus12,
        [Display(Name = "(UTC+12:00) Fiji", ShortName = "Fiji Standard Time")]
        FijiStandardTime,
        [Display(Name = "(UTC+12:00) Petropavlovsk-Kamchatsky - Old", ShortName = "Kamchatka Standard Time")]
        KamchatkaStandardTime,
        [Display(Name = "(UTC+13:00) Nuku'alofa", ShortName = "Tonga Standard Time")]
        TongaStandardTime,
        [Display(Name = "(UTC+13:00) Samoa", ShortName = "Samoa Standard Time")]
        SamoaStandardTime,
        [Display(Name = "(UTC+14:00) Kiritimati Island", ShortName = "Line Islands Standard Time")]
        LineIslandsStandardTime
    }
}
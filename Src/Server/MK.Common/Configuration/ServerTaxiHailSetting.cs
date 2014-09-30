namespace MK.Common.Configuration
{
    public class ServerTaxiHailSetting : TaxiHailSetting
    {
        public ServerTaxiHailSetting() : base()
        {
            // default values here
//            Smtp = "smtpcorp.com";
        }

        public Smtp Smtp { get; private set; }
    }

    public class Smtp
    {
        public string Host { get; private set; }
    }
}
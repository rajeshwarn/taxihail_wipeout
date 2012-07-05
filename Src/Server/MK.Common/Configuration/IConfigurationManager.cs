namespace apcurium.MK.Common.Configuration
{
    public interface IConfigurationManager
    {
        string GetSetting( string key );
        void SetSetting( string key, string value );
    }
}

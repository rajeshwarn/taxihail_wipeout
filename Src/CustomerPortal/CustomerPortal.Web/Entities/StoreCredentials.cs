namespace CustomerPortal.Web.Entities
{
    public class AppleStoreCredentials : StoreCredentials
    {
        public string Team { get; set; }
    }

    public class AndroidStoreCredentials : StoreCredentials
    {
        public string KeystoreMD5Signature { get; set; }
        public string KeystoreSHA1Signature { get; set; }
    }

    public class StoreCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
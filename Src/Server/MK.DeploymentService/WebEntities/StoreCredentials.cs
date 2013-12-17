namespace CustomerPortal.Web.Entities
{
	public class AppleStoreCredentials : StoreCredentials
	{
		public string Team { get; set; }
	}

    public class StoreCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
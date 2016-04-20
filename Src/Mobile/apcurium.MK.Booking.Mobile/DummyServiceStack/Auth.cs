namespace MK.Common.DummyServiceStack
{
    public class Auth
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }


    public class AuthResponse
    {
        public string UserName { get; set; }
        public string SessionId { get; set; }
    }
}
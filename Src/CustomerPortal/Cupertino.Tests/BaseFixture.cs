namespace Cupertino.Tests
{
    public class BaseFixture
    {
        protected string TestUsername = "apcurium";
        protected string TestPassword = "Apcurium52!";
        protected string TestTeam = "MoveOn";
        protected string TestAppId = "com.moveonsoftware.timeon";

        protected Agent agent;

        protected BaseFixture()
        {
            agent = new Agent();
        }
    }
}
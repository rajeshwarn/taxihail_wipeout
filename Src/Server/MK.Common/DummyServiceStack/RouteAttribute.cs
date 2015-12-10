using System;

namespace ServiceStack.ServiceHost
{
    /// <summary>
    /// This is a dummy RouteAttribute to ensure that we can still use the same file on the server.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RouteAttribute : Attribute
    {
        public string Address { get; private set; }
        public string Method { get; private set; }

        public RouteAttribute(string address, string method = null)
        {
            Address = address;
            Method = method;
        }
    }
}
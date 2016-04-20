using System;

namespace apcurium.MK.Common.Http
{
    /// <summary>
    /// This is a dummy RouteAttribute to ensure that we can still use the same file on the server.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RouteDescriptionAttribute : Attribute
    {
        public string Address { get; private set; }
        public string Method { get; private set; }

        public RouteDescriptionAttribute(string address, string method = null)
        {
            Address = address;
            Method = method;
        }
    }
}
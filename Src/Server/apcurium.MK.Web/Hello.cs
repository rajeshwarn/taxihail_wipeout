using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Web
{
    public class Hello
    {
        public string Name { get; set; }
    }

    public class HelloResponse
    {
        public string Result { get; set; }
    }

    public class HelloService : IService<Hello>
    {
        public object Execute(Hello request)
        {
            return new HelloResponse { Result = "Hello, " + request.Name };
        }
    }
}
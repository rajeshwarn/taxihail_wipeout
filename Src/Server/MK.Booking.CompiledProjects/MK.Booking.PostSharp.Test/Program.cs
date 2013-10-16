using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Booking.PostSharp.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            var s = new StaticDataservice();
            var l = s.GetProviders("taxi", "test");
            l.ToString();
            Console.ReadLine();

        }
    }
}

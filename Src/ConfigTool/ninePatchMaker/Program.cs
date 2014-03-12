using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ninePatchMaker
{
    class Program
    {
        static void Main(string[] args)
        {
			var fileName = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Desktop), "splash.png");

			NinePatchMaker.Generate(fileName, Environment.GetFolderPath (Environment.SpecialFolder.Desktop), "splash.91.png");

            
        }


    }
}

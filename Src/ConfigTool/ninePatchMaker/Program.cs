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
            var fileName = @"C:\Users\christopher.lamothe\Desktop\TestSplashScreen\TestSplashScreen\Resources\splash.png";

            NinePatchMaker.Generate(fileName, @"C:\Users\christopher.lamothe\Desktop\TestSplashScreen\TestSplashScreen\Resources\","splash.png");

            
        }


    }
}

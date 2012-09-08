using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace apcurium.MK.Booking.ConfigTool
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            var fullPath = Path.GetFullPath(ToolSettings.Default.RootDirectory);            
            var directories = Directory.GetDirectories(fullPath);
            if (!directories.Any(dir => Path.GetFileName(dir).ToLower() == "config") ||
                 !directories.Any(dir => Path.GetFileName(dir).ToLower() == "src"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Cannot find the config and src folder in : " + fullPath +  Environment.NewLine +"Press any key to exit...");
                Console.ReadKey();
                return;
            }

            var configRootFolder = directories.Single( dir => Path.GetFileName(dir).ToLower() == "config") ;  
            var configDirectories = Directory.GetDirectories(configRootFolder);
            var src = directories.Single(dir => Path.GetFileName(dir).ToLower() == "src");

            var config = configDirectories.Select( dir=> new AppConfig(  Path.GetFileName(dir) , dir, src ));
           
            Console.WriteLine("Choose the config to apply : ");
            Console.WriteLine("");

            for (int i = 0; i < config.Count(); i++)
			{
                Console.WriteLine( i.ToString() + " - " + config.ElementAt(i).Name );			 
			}
            Console.WriteLine("");
            Console.WriteLine("Enter the config number:");
            var selectedText = Console.ReadLine();

            int selected;

            if (int.TryParse(selectedText, out selected) && selected >= 0 && selected < config.Count())
            {
                config.ElementAt(selected).Apply();
            }
            else
            {
                Console.WriteLine("Invalid config selected. Press any key to exit...");
                Console.ReadKey();
                return;
            }
            

            

            
            Console.WriteLine("Done. Closing...");
            Thread.Sleep(5000);            


        }
    }
}

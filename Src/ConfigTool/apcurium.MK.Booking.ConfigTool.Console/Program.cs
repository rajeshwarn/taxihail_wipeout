using System;
using System.Linq;
using System.IO;
using System.Threading;
using apcurium.MK.Booking.ConfigTool.ServiceClient;

namespace apcurium.MK.Booking.ConfigTool
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {

			Console.WriteLine("Starting");
            try
            {

                var c = new CompanyServiceClient();
                var companies =  c.GetCompanies();


				var fullPath = Path.GetFullPath(PathConverter.Convert(ToolSettings.Default.RootDirectory));
                var directories = Directory.GetDirectories(fullPath);
                if (!directories.Any(dir => Path.GetFileName(dir).ToLower() == "config") ||
                    !directories.Any(dir => Path.GetFileName(dir).ToLower() == "src"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Cannot find the config and src folder in : " + fullPath + Environment.NewLine +
                                      "Press any key to exit...");                    
                    return 1;
                }

                var configRootFolder = directories.Single(dir => Path.GetFileName(dir).ToLower() == "config");                                
                var src = directories.Single(dir => Path.GetFileName(dir).ToLower() == "src");                
                var common = Directory.GetDirectories(configRootFolder).Single(name => Path.GetFileName(name).ToLower().Equals("common"));

                //TODO : Instead of reading all the directories in the config folder, we need to build                 
                var config = companies.Select(company => new AppConfig( company.CompanyName ,company, src, common)).ToArray();

                if (args.Length > 0)
                {
                    var configSelected = config.FirstOrDefault(x => x.Name == args[0]);
                    if(configSelected != null)
                    {
                        configSelected.Apply();
                    }else
                    {
                        Console.WriteLine("Invalid config selected. Press any key to exit...");                        
                        return 1;
                    }
                }
                else
                {
                    Console.WriteLine("Choose the config to apply : ");
                    Console.WriteLine("");

                    for (int i = 0; i < config.Count(); i++)
                    {
                        Console.WriteLine(i.ToString() + " - " + config.ElementAt(i).Name);
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
                        //Console.ReadKey();
                        return 1;
                    }
                }

                Console.WriteLine("Done. Closing...");
                Thread.Sleep(2000);

          }catch(Exception e)
          {
			  Console.WriteLine("Errors:");
			  Console.WriteLine(e.Message);
			  //Console.ReadKey();
	          return 1;
          }
			Console.WriteLine("Finished");
          return 0;

        }
    }
}

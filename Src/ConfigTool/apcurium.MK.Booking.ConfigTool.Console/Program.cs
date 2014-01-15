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
                if (!directories.Any(dir => Path.GetFileName(dir).ToLower() == "src"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Cannot find the src folder in : " + fullPath + Environment.NewLine +
                                      "Press any key to exit...");                    
                    return 1;
                }

                                               
                var src = directories.Single(dir => Path.GetFileName(dir).ToLower() == "src");
				var configFolders = Path.Combine(fullPath, "Config");
				var config = companies.Select(company => new AppConfig( company.CompanyKey ,company, src, Path.Combine(configFolders, company.CompanyKey))).ToArray();

				if (args.Length >= 2)
                {
                    var configSelected = config.FirstOrDefault(x => x.Name == args[0]);
					var serviceUrl = args[1];
                    if(configSelected != null)
                    {
						configSelected.Apply(serviceUrl);
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
						Console.WriteLine(i.ToString() + " - " + config.ElementAt(i).Name + " - " + config.ElementAt(i).Company.CompanyName );
                    }
                    Console.WriteLine("");
                    Console.WriteLine("Enter the config number:");
                    var selectedText = Console.ReadLine();
                    int selected = int.Parse(selectedText);

					Console.WriteLine("Enter the server url:");
					var url = Console.ReadLine();

					if (selected > 0 && !string.IsNullOrWhiteSpace(url))
                    {
						config.ElementAt(selected).Apply(url);
                    }
                    else
                    {
                        Console.WriteLine("Invalid config selected or bad service url. Press any key to exit...");
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

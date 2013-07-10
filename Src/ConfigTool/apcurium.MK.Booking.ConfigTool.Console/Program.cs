using System;
using System.Linq;
using System.IO;
using System.Threading;

namespace apcurium.MK.Booking.ConfigTool
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            try
            {

				var fullPath = Path.GetFullPath(PathConverter.Convert(ToolSettings.Default.RootDirectory));
                var directories = Directory.GetDirectories(fullPath);
                if (!directories.Any(dir => Path.GetFileName(dir).ToLower() == "config") ||
                    !directories.Any(dir => Path.GetFileName(dir).ToLower() == "src"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Cannot find the config and src folder in : " + fullPath + Environment.NewLine +
                                      "Press any key to exit...");
                    //Console.ReadKey();
                    return 1;
                }

                var configRootFolder = directories.Single(dir => Path.GetFileName(dir).ToLower() == "config");
                var configDirectories = Directory.GetDirectories(configRootFolder).Where(name => !Path.GetFileName(name).ToLower().Equals("common"));
                var src = directories.Single(dir => Path.GetFileName(dir).ToLower() == "src");
                var common = Directory.GetDirectories(configRootFolder).Single(name => Path.GetFileName(name).ToLower().Equals("common"));

                var config = configDirectories.Select(dir => new AppConfig(Path.GetFileName(dir), dir, src, common)).ToArray();

                if (args.Length > 0)
                {
                    var configSelected = config.FirstOrDefault(x => x.Name == args[0]);
                    if(configSelected != null)
                    {
                        configSelected.Apply();
                    }else
                    {
                        Console.WriteLine("Invalid config selected. Press any key to exit...");
                        //Console.ReadKey();
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
          return 0;

        }
    }
}

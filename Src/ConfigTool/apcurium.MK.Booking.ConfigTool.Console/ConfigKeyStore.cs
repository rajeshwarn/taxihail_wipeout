using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace apcurium.MK.Booking.ConfigTool
{
    public class ConfigKeyStore : Config
    {
        public ConfigKeyStore(AppConfig parent)
            : base(parent)
        {
        }

        public override void Apply()
        {
            string keystoreFile = Path.Combine(Parent.ConfigDirectoryPath , "public.keystore");
            if (!File.Exists(keystoreFile))
            {
               CreateKeyStore(keystoreFile);
            }

            //if ( string.IsNullOrEmpty( Parent.App.
            //{
                                
                GetMD5Fingerprint( keystoreFile );
                //keytool -list -alias alias_name -keystore my-release-key.keystore
            //}

        }

        private void GetMD5Fingerprint (string keystoreFile)
        {
            if ( !string.IsNullOrEmpty( Parent.App.GoogleMapKey))
            {
                return;
            }
            Console.WriteLine("");
            Console.Write("Creating MD5 Fingerprint...");

            

            string command = @" -list -alias {0} -keystore ""{1}"" -storepass {2} -keypass {2}";                           
            //0= Alias name
            //1=D:\Sources\MK Taxi\Config\AtlantaCheckerCab\public.keystore            

            command = string.Format(command, Parent.App.AndroidSigningKeyAlias, keystoreFile, Parent.App.AndroidSigningKeyPassStorePass);

            var process = new Process();
            var startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = @"C:\Program Files (x86)\Java\jdk1.6.0_31\bin\keytool.exe";
            startInfo.Arguments = command;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
             
            
            process.StartInfo = startInfo;
            process.Start();

            Thread.Sleep(300);

            var result = process.StandardOutput.ReadToEnd();

            var index = result.IndexOf("(MD5):");

            var md5 = result.Substring( index + 6, result.Length - index - 6 ).Trim();


            Console.WriteLine("");
            Console.WriteLine("You must generate the Google key.  Navigate to this link and copy the result in the appinfo.json file. The link is copied to your clipboard. ");
            Clipboard.SetText("http://www.google.com/glm/mmap/a/api?fp=" + md5);
            Console.WriteLine("http://www.google.com/glm/mmap/a/api?fp=" + md5 );

            Console.WriteLine("Press any key when you are done.");

            Console.ReadKey();
            Parent.UnloadApp();


        }

        private void CreateKeyStore(string keystoreFile)
        {
            Console.WriteLine("");
            Console.Write("Creating keystore...");

            string command = @" -genkey -v -keystore ""{0}"" -alias {1} -keyalg RSA -keysize 2048 -validity 10000 -storepass {2} -dname ""cn={3}"" -keypass {2}";
            //0=D:\Sources\MK Taxi\Config\AtlantaCheckerCab\public.keystore
            //1=AtlantaCheckerCab
            //2=password
            //3=App Name
            command = string.Format(command, keystoreFile, Parent.App.AndroidSigningKeyAlias, Parent.App.AndroidSigningKeyPassStorePass, Parent.App.AppName);

            var process = new Process();
            var startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = @"C:\Program Files (x86)\Java\jdk1.6.0_31\bin\keytool.exe"; ;
            startInfo.Arguments = command;
            process.StartInfo = startInfo;
            process.Start();


            while (!File.Exists(keystoreFile))
            {
                Console.Write(".");
                Thread.Sleep(500);
            }




            Console.WriteLine("");
            Console.WriteLine("Keystore created.");
        }
    }
}

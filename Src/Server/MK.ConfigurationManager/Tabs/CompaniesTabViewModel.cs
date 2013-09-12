using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MK.ConfigurationManager.Annotations;
using MK.ConfigurationManager.Entities;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace MK.ConfigurationManager.Tabs
{
    public class CompaniesTabViewModel : INotifyPropertyChanged
    {
        public CompaniesTabViewModel()
        {
            ConfigurationProperties = new ObservableCollection<MyCustomKeyValuePair>();
            MobileConfigurationProperties = new ObservableCollection<MyCustomKeyValuePair>();
            CurrentCompany = Companies.FirstOrDefault();
        }

        public ObservableCollection<MyCustomKeyValuePair> ConfigurationProperties { get; set; }
        public ObservableCollection<MyCustomKeyValuePair> MobileConfigurationProperties { get; set; }
        public ObservableCollection<Company> Companies { get { return ConfigurationDatabase.Current.Companies; } }

            
        private Company _currentCompany;
        public Company CurrentCompany
        {
            get { return _currentCompany; }
            set
            {
                if (value == null) return;

                _currentCompany = value;
                OnPropertyChanged();
                ConfigurationProperties.Clear();
                CurrentCompany.ConfigurationProperties.OrderBy(x => x.Key).ToList().ForEach(x => ConfigurationProperties.Add(new MyCustomKeyValuePair(x.Key, x.Value)));
                MobileConfigurationProperties.Clear();
                CurrentCompany.MobileConfigurationProperties.OrderBy(x => x.Key).ToList().ForEach(x => MobileConfigurationProperties.Add(new MyCustomKeyValuePair(x.Key, x.Value)));
            }
        }

        public void SaveCompany(object sender, RoutedEventArgs e)
        {
            CurrentCompany.ConfigurationProperties = ConfigurationProperties.ToDictionary(x => x.Key, y => y.Value);
            CurrentCompany.MobileConfigurationProperties = MobileConfigurationProperties.ToDictionary(x => x.Key, y => y.Value);
            ConfigurationDatabase.Current.SaveCompanies();
        }

        static public string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public void addCompabyBt_Click(object sender, RoutedEventArgs e)
        {
            var newCompany = new Company { Id = Guid.NewGuid() };
            var jsonSettings = File.ReadAllText(Path.Combine(AssemblyDirectory, "Entities\\CompanyTemplate.json"));
            var objectSettings = JObject.Parse(jsonSettings);

            foreach (var token in objectSettings)
            {
                newCompany.ConfigurationProperties.Add(token.Key, token.Value.ToString());
            }

            jsonSettings = File.ReadAllText(Path.Combine(AssemblyDirectory, "Entities\\CompanyMobileTemplate.json"));
            objectSettings = JObject.Parse(jsonSettings);

            foreach (var token in objectSettings)
            {
                newCompany.MobileConfigurationProperties.Add(token.Key, token.Value.ToString());
            }
            ConfigurationDatabase.Current.AddCompany(newCompany);

            ConfigurationProperties.Clear();
            MobileConfigurationProperties.Clear();
            CurrentCompany = newCompany;
        }


        public void GenerateKeyStoreAndMapKey_Click(object sender, RoutedEventArgs e)
        {
            //generate key store file
            var command = @" -genkey -v -keystore ""{0}"" -alias {1} -keyalg RSA -keysize 2048 -validity 10000 -storepass {2} -dname ""cn={3}"" -keypass {2}";
            var keystoreFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "public.keystore");

            command = String.Format(command, keystoreFile, MobileConfigurationProperties.First(x => x.Key == "AndroidSigningKeyAlias").Value, MobileConfigurationProperties.First(x => x.Key == "AndroidSigningKeyPassStorePass").Value, CurrentCompany.Name);


            string pathToKeyTool = FindKeytoolPath();

            var generateKeyTool = new ProcessStartInfo
            {
                FileName = pathToKeyTool,
                Arguments = command
            };

            using (var exeProcess = Process.Start(generateKeyTool))
            {
                exeProcess.WaitForExit();
            }

            //genete md5 fingerprint for google map key
            var commandMD5 = @"-v -list -alias {0} -keystore ""{1}"" -storepass {2} -keypass {2}";
            commandMD5 = String.Format(commandMD5,
                                       MobileConfigurationProperties.First(x => x.Key == "AndroidSigningKeyAlias").Value,
                                       keystoreFile,
                                       MobileConfigurationProperties.First(
                                           x => x.Key == "AndroidSigningKeyPassStorePass").Value);

            var generateMD5 = new ProcessStartInfo
            {
                FileName = pathToKeyTool,
                Arguments = commandMD5,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            using (var exeProcess = Process.Start(generateMD5))
            {
                exeProcess.WaitForExit();
                var result = exeProcess.StandardOutput.ReadToEnd();
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during google map key generation");
                }
                //todo
                //OutputTextBox.Text = "Key store generated on the desktop ... Google Map Key Generate, link in clipboard";
                var index = result.IndexOf("MD5:");
                var md5 = result.Substring(index + 6, 47).Trim();
                Console.WriteLine("");
                Console.WriteLine("You must generate the Google key.  Navigate to this link and copy the result in the appinfo.json file. The link is copied to your clipboard. ");
                try
                {
                    Clipboard.SetText("http://www.google.com/glm/mmap/a/api?fp=" + md5);
                }
                catch { }
                Process.Start("http://www.google.com/glm/mmap/a/api?fp=" + md5);
                if (MobileConfigurationProperties.All(x => x.Key != "GoogleMapKey"))
                {
                    MobileConfigurationProperties.Add(new MyCustomKeyValuePair("GoogleMapKey", null));
                }
            }
        }


        public void ImportMobileSettings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            var result = dlg.ShowDialog();
            if (result == true)
            {
                var filename = dlg.FileName;
                var jsonSettings = File.ReadAllText(filename);
                var objectSettings = JObject.Parse(jsonSettings);

                foreach (var token in objectSettings)
                {
                    if (MobileConfigurationProperties.Any(x => x.Key == token.Key))
                    {
                        MobileConfigurationProperties.First(x => x.Key == token.Key).Value = token.Value.ToString();
                    }
                    else
                    {
                        MobileConfigurationProperties.Add(new MyCustomKeyValuePair(token.Key, token.Value.ToString()));
                    }

                }
            }

        }

        private static string FindKeytoolPath()
        {
            var p = new Process
                {
                    StartInfo =
                        {
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            FileName = "cmd.exe"
                        }
                };
            p.Start();
            p.StandardInput.WriteLine("where /R \"c:\\Program Files (x86)\\Java\" keytool");
            p.StandardInput.WriteLine("exit");

            var lines = new List<string>();
            while (!p.StandardOutput.EndOfStream)
            {
                var line = p.StandardOutput.ReadLine();
                lines.Add(line);
            }

            var keytoolPath = lines.Last(x => x.EndsWith("keytool.exe"));
            Debug.WriteLine(keytoolPath);
            return keytoolPath;
        }

        public void Refresh()
        {
            ConfigurationDatabase.Current.ReloadDeployments();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}

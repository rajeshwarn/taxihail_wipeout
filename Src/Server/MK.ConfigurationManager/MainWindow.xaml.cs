using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using MK.ConfigurationManager.Entities;
using Newtonsoft.Json.Linq;

namespace MK.ConfigurationManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Company _currentCompany;
        ConfigurationManagerDbContext DbContext { get; set; }

        public ObservableCollection<Company> Companies { get; set; }

        public ObservableCollection<IBSServer> IBSServers { get; set; }

        public ObservableCollection<TaxiHailEnvironment> TaxiHailEnvironments { get; set; }

        public ObservableCollection<AppVersion> Versions { get; set; }

        public ObservableCollection<MyCustomKeyValuePair> ConfigurationProperties { get; set; }

        public ObservableCollection<MyCustomKeyValuePair> MobileConfigurationProperties { get; set; }

        public ObservableCollection<DeploymentJob> DeploymentJobs { get; set; } 

        public Company CurrentCompany
        {
            get { return _currentCompany; }
            set
            {
                if (value != null)
                {
                    _currentCompany = value;
                    OnPropertyChanged("CurrentCompany");
                    ConfigurationProperties.Clear();
                    CurrentCompany.ConfigurationProperties.OrderBy(x => x.Key).ToList().ForEach(x => ConfigurationProperties.Add(new MyCustomKeyValuePair(x.Key, x.Value)));
                    MobileConfigurationProperties.Clear();
                    CurrentCompany.MobileConfigurationProperties.OrderBy(x => x.Key).ToList().ForEach(x => MobileConfigurationProperties.Add(new MyCustomKeyValuePair(x.Key, x.Value)));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindowLoaded;

            this.DataContext = this;

            Companies = new ObservableCollection<Company>();
            ConfigurationProperties = new ObservableCollection<MyCustomKeyValuePair>();
            MobileConfigurationProperties = new ObservableCollection<MyCustomKeyValuePair>();
            IBSServers = new ObservableCollection<IBSServer>();
            TaxiHailEnvironments = new ObservableCollection<TaxiHailEnvironment>();
            DeploymentJobs = new ObservableCollection<DeploymentJob>();
            Versions = new ObservableCollection<AppVersion>();

            AutoRefreshCheckbox.Checked += AutoRefreshCheckbox_Checked;

        }

        void AutoRefreshCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (AutoRefreshCheckbox.IsChecked.HasValue && AutoRefreshCheckbox.IsChecked.Value)
            {
                AutoRefresh();
            }
        }

        private void AutoRefresh()
        {
            Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(_ => 
            {
                RefreshData();
                if (AutoRefreshCheckbox.IsChecked.HasValue && AutoRefreshCheckbox.IsChecked.Value)
                {
                    AutoRefresh();
                }
            });
        }

        void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void updateCS_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            
            int selectedCompanyIndex = DeployCompanyCombobox.SelectedIndex;
            int selectedIbsServerIndex = DeployIbsServerCombobox.SelectedIndex;
            int selectedTaxiHailEnvIndex = DeployTaxiHailEnvCombobox.SelectedIndex;

            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString;
            if (this.currentDbList.SelectedItem != null)
            {
                connectionString = (this.currentDbList.SelectedItem as ComboBoxItem).Content.ToString();
            }
            DbContext = new ConfigurationManagerDbContext(connectionString);
            DbContext.Database.CreateIfNotExists();
           
            
            Companies.Clear();
            DbContext.Set<Company>().OrderBy(x => x.Name).ToList().ForEach(Companies.Add);

            IBSServers.Clear();
            DbContext.Set<IBSServer>().OrderBy(x => x.Name).ToList().ForEach(IBSServers.Add);
            IBSServers.CollectionChanged += IBSServersCollectionChanged;

            TaxiHailEnvironments.Clear();
            DbContext.Set<TaxiHailEnvironment>().ToList().ForEach(TaxiHailEnvironments.Add);
            TaxiHailEnvironments.CollectionChanged += TaxiHailEnvironmentsOnCollectionChanged;

            Versions.Clear();
            DbContext.Set<AppVersion>().ToList().ForEach(Versions.Add);
            Versions.CollectionChanged += VersionsOnCollectionChanged;


            DeploymentJobs.Clear();
            DbContext.Set<DeploymentJob>().OrderByDescending(x => x.RequestedDate).ToList().ForEach(DeploymentJobs.Add);
            statusBarTb.Text = "Done";

            DeployCompanyCombobox.SelectedIndex = selectedCompanyIndex;
            DeployIbsServerCombobox.SelectedIndex = selectedIbsServerIndex;
            DeployTaxiHailEnvCombobox.SelectedIndex = selectedTaxiHailEnvIndex;

        }



        private void VersionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                e.NewItems.OfType<AppVersion>().ToList().ForEach(x =>
                {
                    if(x.Id == Guid.Empty)
                    {
                        x.Id = Guid.NewGuid();
                        DbContext.Set<AppVersion>().Add(x);
                    }
                });
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                e.OldItems.OfType<AppVersion>().ToList().ForEach(x =>
                {
                    
                        DbContext.Set<AppVersion>().Remove(x);
                    
                });
            }
        }

        private void TaxiHailEnvironmentsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                e.NewItems.OfType<TaxiHailEnvironment>().ToList().ForEach(x =>
                {
                    if(x.Id == Guid.Empty)
                    {
                        x.Id = Guid.NewGuid();
                        DbContext.Set<TaxiHailEnvironment>().Add(x);
                    }
                });
            }
        }

        void IBSServersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
               e.NewItems.OfType<IBSServer>().ToList().ForEach(x =>
                {
                    if(x.Id == Guid.Empty)
                    {
                        x.Id = Guid.NewGuid();
                        DbContext.Set<IBSServer>().Add(x);
                    }
                });
            }
        }

        private void addCompabyBt_Click(object sender, RoutedEventArgs e)
        {
            var newCompany = new Company {Id = Guid.NewGuid()};
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

            DbContext.Set<Company>().Add(newCompany);
            Companies.Add(newCompany);
            ConfigurationProperties.Clear();
            MobileConfigurationProperties.Clear();
            CurrentCompany = newCompany;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SaveCompany(object sender, RoutedEventArgs e)
        {
            CurrentCompany.ConfigurationProperties = ConfigurationProperties.ToDictionary(x => x.Key, y => y.Value);
            CurrentCompany.MobileConfigurationProperties = MobileConfigurationProperties.ToDictionary(x => x.Key, y => y.Value);
            DbContext.SaveChanges();
        }

        private void SaveIBSServer(object sender, RoutedEventArgs e)
        {
            DbContext.SaveChanges();
        }

        private void SaveTaxiHailEnvironment(object sender, RoutedEventArgs e)
        {
            DbContext.SaveChanges();
        }

        private void SaveVersion(object sender, RoutedEventArgs e)
        {
            DbContext.SaveChanges();
        }

        static public string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public Company DeployCompany { get; set; }
        public IBSServer DeployIBSServer { get; set; }
        public TaxiHailEnvironment DeployTaxiHailEnv { get; set; }
        public AppVersion DeployVersion { get; set; }

        public bool DeployInitDatabse { get; set; }
        public bool DeployServer { get; set; }
        public bool DeployIosAdHoc { get; set; }
        public bool DeployIosAppStore { get; set; }
        public bool DeployAndroid { get; set; }
        public bool DeployCallBox { get; set; }
        public bool DeployDB { get; set; }
        public string DeployRevision { get; set; }
        

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var job = new DeploymentJob();
            job.RequestedDate = DateTime.Now;
            job.Id = Guid.NewGuid();
            job.Company = DeployCompany;
            job.IBSServer = DeployIBSServer;
            job.Revision = DeployRevision;
            job.Version = DeployVersion;
            job.TaxHailEnv = DeployTaxiHailEnv;
            job.DeployDB = DeployDB;
            job.InitDatabase = DeployInitDatabse;
            job.Android = DeployAndroid;
            job.CallBox = DeployCallBox;
            job.DeployServer = DeployServer;
            job.iOS_AdHoc = DeployIosAdHoc;
            job.iOS_AppStore = DeployIosAppStore;
            job.Status = JobStatus.REQUESTED;
            DbContext.Set<DeploymentJob>().Add(job);
            DbContext.SaveChanges();
            DeploymentJobs.Add(job);
        }

        private void RefreshDeployments(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void GenerateKeyStoreAndMapKey_Click(object sender, RoutedEventArgs e)
        {
            //generate key store file
            var command = @" -genkey -v -keystore ""{0}"" -alias {1} -keyalg RSA -keysize 2048 -validity 10000 -storepass {2} -dname ""cn={3}"" -keypass {2}";
            var keystoreFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "public.keystore") ;

            command = string.Format(command, keystoreFile, MobileConfigurationProperties.First(x => x.Key == "AndroidSigningKeyAlias").Value, MobileConfigurationProperties.First(x => x.Key == "AndroidSigningKeyPassStorePass").Value, CurrentCompany.Name);


            const string pathToKeyToo = @"C:\Program Files (x86)\Java\jdk1.6.0_31\bin\keytool.exe";
            
            var generateKeyTool = new ProcessStartInfo
                                {
                                    FileName = pathToKeyToo,
                                    Arguments = command
                                };
            using (var exeProcess = Process.Start(generateKeyTool))
            {
                exeProcess.WaitForExit();
                if (exeProcess.ExitCode > 0)
                {
                    statusBarTb.Text = "Error during keystore file";
                }else
                {
                    statusBarTb.Text = "Key store generated on the desktop ... Generating Google Map Key";
                }
            }

            //genete md5 fingerprint for google map key
            var commandMD5 = @"-v -list -alias {0} -keystore ""{1}"" -storepass {2} -keypass {2}";
            commandMD5 = string.Format(commandMD5,
                                       MobileConfigurationProperties.First(x => x.Key == "AndroidSigningKeyAlias").Value,
                                       keystoreFile,
                                       MobileConfigurationProperties.First(
                                           x => x.Key == "AndroidSigningKeyPassStorePass").Value);

            var generateMD5 = new ProcessStartInfo
            {
                FileName = pathToKeyToo,
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
                    statusBarTb.Text = "Error during google map key generation";
                }
                else
                {
                    statusBarTb.Text = "Key store generated on the desktop ... Google Map Key Generate, link in clipboard";
                    var index = result.IndexOf("MD5:");
                    var md5 = result.Substring(index + 6, 47 ).Trim();
                    Console.WriteLine("");
                    Console.WriteLine("You must generate the Google key.  Navigate to this link and copy the result in the appinfo.json file. The link is copied to your clipboard. ");
                    Clipboard.SetText("http://www.google.com/glm/mmap/a/api?fp=" + md5);
                    Process.Start("http://www.google.com/glm/mmap/a/api?fp=" + md5);
                    if(MobileConfigurationProperties.All(x => x.Key != "GoogleMapKey"))
                    {
                        MobileConfigurationProperties.Add(new MyCustomKeyValuePair("GoogleMapKey", null));
                    }
                }
            }
        }

        

        private void ImportMobileSettings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
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
                    }else
                    {
                        MobileConfigurationProperties.Add(new MyCustomKeyValuePair(token.Key, token.Value.ToString()));
                    }
                    
                }
            }

        }

        private void ClearDeployHistory(object sender, RoutedEventArgs e)
        {
            var listDeploySelected = DeployDataGrid.SelectedItems;
            foreach (DeploymentJob job in listDeploySelected)
            {
                DbContext.Set<DeploymentJob>().Remove(job);
            }
            DbContext.SaveChanges();
            this.RefreshData();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
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

        public ObservableCollection<MyCustomKeyValuePair> ConfigurationProperties { get; set; }

        public ObservableCollection<DeploymentJob> DeploymentJobs { get; set; } 

        public Company CurrentCompany
        {
            get { return _currentCompany; }
            set
            {
                _currentCompany = value;
                OnPropertyChanged("CurrentCompany");
                ConfigurationProperties.Clear(); 
                CurrentCompany.ConfigurationProperties.ToList().ForEach(x => ConfigurationProperties.Add(new MyCustomKeyValuePair(x.Key, x.Value)));
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindowLoaded;

            this.DataContext = this;

            Companies = new ObservableCollection<Company>();
            ConfigurationProperties = new ObservableCollection<MyCustomKeyValuePair>();
            IBSServers = new ObservableCollection<IBSServer>();
            TaxiHailEnvironments = new ObservableCollection<TaxiHailEnvironment>();
            DeploymentJobs = new ObservableCollection<DeploymentJob>();
        }

        void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            //DbContext.Database.CreateIfNotExists();
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ConfigurationManagerDbContext, SimpleDbMigrationsConfiguration>("MKConfig"));
            DbContext = new ConfigurationManagerDbContext(System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString);
            this.currentDbCs.Text = System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString;
            
            DbContext.Set<Company>().ToList().ForEach(Companies.Add);

            DbContext.Set<IBSServer>().ToList().ForEach(IBSServers.Add);
            IBSServers.CollectionChanged += IBSServersCollectionChanged;

            DbContext.Set<TaxiHailEnvironment>().ToList().ForEach(TaxiHailEnvironments.Add);
            TaxiHailEnvironments.CollectionChanged += TaxiHailEnvironmentsOnCollectionChanged;

            DbContext.Set<DeploymentJob>().ToList().ForEach(DeploymentJobs.Add);
        }

        private void TaxiHailEnvironmentsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                e.NewItems.OfType<TaxiHailEnvironment>().ToList().ForEach(x =>
                {
                    x.Id = Guid.NewGuid();
                    DbContext.Set<TaxiHailEnvironment>().Add(x);
                });
            }
        }

        void IBSServersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
               e.NewItems.OfType<IBSServer>().ToList().ForEach(x =>
                                                                   {
                                                                       x.Id = Guid.NewGuid();
                                                                       DbContext.Set<IBSServer>().Add(x);
                                                                   });
            }
        }

        private void addCompabyBt_Click(object sender, RoutedEventArgs e)
        {
            var newCompany = new Company() {Id = Guid.NewGuid()};
            var jsonSettings = File.ReadAllText(Path.Combine(AssemblyDirectory, "Entities\\Companytemplate.json"));
            var objectSettings = JObject.Parse(jsonSettings);

            foreach (var token in objectSettings)
            {
                newCompany.ConfigurationProperties.Add(token.Key, token.Value.ToString());
            }

            DbContext.Set<Company>().Add(newCompany);
            Companies.Add(newCompany);
            ConfigurationProperties.Clear();
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
        public bool DeployInitDatabse { get; set; }
        public bool DeployServer { get; set; }
        public bool DeployIos { get; set; }
        public bool DeployAndroid { get; set; }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var job = new DeploymentJob();
            job.Id = Guid.NewGuid();
            job.Company = DeployCompany;
            job.IBSServer = DeployIBSServer;
            job.TaxHailEnv = DeployTaxiHailEnv;
            job.InitDatabase = DeployInitDatabse;
            job.Android = DeployAndroid;
            job.Server = DeployServer;
            job.iOS = DeployIos;
            job.Status = JobStatus.REQUESTED;
            DbContext.Set<DeploymentJob>().Add(job);
            DbContext.SaveChanges();
            DeploymentJobs.Add(job);
        }

        private void RefreshDeployments(object sender, RoutedEventArgs e)
        {
            DeploymentJobs.Clear();
            DbContext.Set<DeploymentJob>().ToList().ForEach(DeploymentJobs.Add);
        }
    }

    class SimpleDbMigrationsConfiguration  : DbMigrationsConfiguration<ConfigurationManagerDbContext>
    {
        public SimpleDbMigrationsConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            
        }
    }

    public class MyCustomKeyValuePair
    {
        public MyCustomKeyValuePair()
        {
            
        }

        public MyCustomKeyValuePair(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }

}

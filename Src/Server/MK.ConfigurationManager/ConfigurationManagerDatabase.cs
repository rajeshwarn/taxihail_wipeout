using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MK.ConfigurationManager.Entities;

namespace MK.ConfigurationManager
{
    class ConfigurationDatabase
    {
        private ConfigurationDatabase()
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString;

            DbContext = new ConfigurationManagerDbContext(connectionString);
            DbContext.Database.CreateIfNotExists();

            DeploymentJobs = new ObservableCollection<DeploymentJob>();

            IBSServers = new ObservableCollection<IBSServer>();
            IBSServers.CollectionChanged += IBSServersCollectionChanged;
            
            TaxiHailEnvironments = new ObservableCollection<TaxiHailEnvironment>();
            TaxiHailEnvironments.CollectionChanged += TaxiHailEnvironmentsOnCollectionChanged;

            Companies = new ObservableCollection<Company>();
            Companies.CollectionChanged += TaxiHailEnvironmentsOnCollectionChanged;

            Versions = new ObservableCollection<AppVersion>();
            Versions.CollectionChanged += VersionsOnCollectionChanged;


            Load();
        }

        public ObservableCollection<IBSServer> IBSServers { get; set; }
        public ObservableCollection<TaxiHailEnvironment> TaxiHailEnvironments { get; set; }
        public ObservableCollection<AppVersion> Versions { get; set; }
        public ObservableCollection<DeploymentJob> DeploymentJobs { get; set; }

        public ObservableCollection<Company> Companies { get; set; }

        private void Load()
        {
            IBSServers.Clear();
            DbContext.Set<IBSServer>().OrderBy(x => x.Name).ToList().ForEach(IBSServers.Add);

            TaxiHailEnvironments.Clear();
            DbContext.Set<TaxiHailEnvironment>().ToList().ForEach(TaxiHailEnvironments.Add);

            Versions.Clear();
            DbContext.Set<AppVersion>().OrderBy(x => x.Display).ToList().ForEach(Versions.Add);
            ReloadVersions();
            ReloadCompanies();

            ReloadDeployments();

        }

        private void ReloadCompanies()
        {
            Companies.Clear();
            DbContext.Set<Company>().OrderBy(x => x.Name).ToList().ForEach(Companies.Add);
        }

        public  ConfigurationManagerDbContext DbContext { get; set; }

        private static ConfigurationDatabase _instance;
        public static ConfigurationDatabase Current
        {
            get { return _instance ?? (_instance = new ConfigurationDatabase()); }
        }

        private void VersionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                e.NewItems.OfType<AppVersion>().ToList().ForEach(x =>
                {
                    if (x.Id != Guid.Empty) return;

                    x.Id = Guid.NewGuid();
                    DbContext.Set<AppVersion>().Add(x);
                });
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                e.OldItems.OfType<AppVersion>().ToList().ForEach(x => DbContext.Set<AppVersion>().Remove(x));
            }
        }

        private void TaxiHailEnvironmentsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                e.NewItems.OfType<TaxiHailEnvironment>().ToList().ForEach(x =>
                {
                    if (x.Id != Guid.Empty) return;
                    x.Id = Guid.NewGuid();
                    DbContext.Set<TaxiHailEnvironment>().Add(x);
                });
            }
        }

        void IBSServersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                e.NewItems.OfType<IBSServer>().ToList().ForEach(x =>
                {
                    if (x.Id != Guid.Empty) return;
                    x.Id = Guid.NewGuid();
                    DbContext.Set<IBSServer>().Add(x);
                });
            }
        }

        
        public void ReloadDeployments()
        {

            DeploymentJobs.Clear();
            DbContext.Set<DeploymentJob>().OrderByDescending(x => x.RequestedDate).ToList().ForEach(DeploymentJobs.Add);
        }

        public void ClearDeployHistory()
        {

            DeploymentJobs.Clear();
            foreach (var job in DbContext.Set<DeploymentJob>())
            {
                if (job.Status == JobStatus.ERROR || job.Status == JobStatus.SUCCESS)
                {
                    DbContext.Set<DeploymentJob>().Remove(job);
                }
            }
            DbContext.SaveChanges();
        }

        internal void AddJob(DeploymentJob job)
        {
            DbContext.Set<DeploymentJob>().Add(job);
            DbContext.SaveChanges();
            DeploymentJobs.Add(job);
        }

  

        internal void AddCompany(Company newCompany)
        {

            DbContext.Set<Company>().Add(newCompany);
            Companies.Add(newCompany); ;
        }

        public void ReloadVersions()
        {
            Versions.Clear();
            DbContext.Set<AppVersion>().OrderBy(x => x.Display).ToList().ForEach(Versions.Add);
        }
    }
}

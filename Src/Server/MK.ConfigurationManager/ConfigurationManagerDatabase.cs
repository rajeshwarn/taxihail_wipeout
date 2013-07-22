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
            
            UseDbContext(context => context.Database.CreateIfNotExists());

            DeploymentJobs = new ObservableCollection<DeploymentJob>();
            IBSServers = new ObservableCollection<IBSServer>();
            TaxiHailEnvironments = new ObservableCollection<TaxiHailEnvironment>();
            Companies = new ObservableCollection<Company>();
            Versions = new ObservableCollection<AppVersion>();

            Load();
        }

        public ObservableCollection<IBSServer> IBSServers { get; set; }
        public ObservableCollection<TaxiHailEnvironment> TaxiHailEnvironments { get; set; }
        public ObservableCollection<AppVersion> Versions { get; set; }
        public ObservableCollection<DeploymentJob> DeploymentJobs { get; set; }

        public ObservableCollection<Company> Companies { get; set; }

        private void Load()
        {
            ReloadEnvironments();

            ReloadIbsServers();

            ReloadVersions();
            ReloadCompanies();

            ReloadDeployments();

        }

        private void ReloadCompanies()
        {
            Companies.Clear();
            UseDbContext(context => context.Set<Company>().OrderBy(x => x.Name).ToList().ForEach(Companies.Add));
            
        }
        
        public void UseDbContext(Action<ConfigurationManagerDbContext> dbOperation)
        {
            using (var context = GetDbContext())
            {
                dbOperation(context);
            }
        } 

        public Func<ConfigurationManagerDbContext> GetDbContext
        {
            get
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString;
                return () => new ConfigurationManagerDbContext(connectionString);
            }
        }

        private static ConfigurationDatabase _instance;
        public static ConfigurationDatabase Current
        {
            get { return _instance ?? (_instance = new ConfigurationDatabase()); }
        }
        
        public void ReloadDeployments()
        {
            UseDbContext(context =>
                {
                    DeploymentJobs.Clear();

                    context.DeploymentJobs
                           .OrderByDescending(x => x.RequestedDate)
                           .ToList()
                           .ForEach(job => DeploymentJobs.Add(job));
                });

        }

        public void ClearDeployHistory()
        {
            UseDbContext(context =>
                {
                    DeploymentJobs.Clear();
                    foreach (var job in context.Set<DeploymentJob>())
                    {
                        if (job.Status == JobStatus.ERROR || job.Status == JobStatus.SUCCESS)
                        {
                            context.Set<DeploymentJob>().Remove(job);
                        }
                    }
                    context.SaveChanges();
                });
        }

        internal void AddJob(DeploymentJob job)
        {
            UseDbContext(context =>
                {
                    job.Company = context.Set<Company>().Find(job.Company.Id);
                    job.IBSServer = context.Set<IBSServer>().Find(job.IBSServer.Id);
                    job.TaxHailEnv = context.Set<TaxiHailEnvironment>().Find(job.TaxHailEnv.Id);
                    if (job.Version != null)
                    {
                        job.Version = context.Set<AppVersion>().Find(job.Version.Id);
                    }
                    context.Set<DeploymentJob>().Add(job);
                    context.SaveChanges();
                });

            DeploymentJobs.Add(job);
        }



        internal void AddCompany(Company newCompany)
        {
            UseDbContext(context =>
                {
                    context.Set<Company>().Add(newCompany);
                    context.SaveChanges();
                });
            Companies.Add(newCompany);

        }

        public void ReloadVersions()
        {
            Versions.Clear();
            UseDbContext(context => context.Set<AppVersion>().OrderBy(x => x.Display).ToList().ForEach(Versions.Add));
        }

       
        public void AddVersion(AppVersion appVersion)
        {
            UseDbContext(context =>
            {
                context.Set<AppVersion>().Add(appVersion);
                context.SaveChanges();
            });
        }

        public void SaveVersions()
        {
            SaveAll(Versions);

            ReloadVersions();
        }
        void SaveAll<T>(IEnumerable<T> entities) where T : class
        {
            UseDbContext(context =>
            {
                foreach (var entity in entities)
                {
                    context.Set<T>().Attach(entity);
                    context.Entry(entity).State = EntityState.Modified;
                }
                context.SaveChanges();
            });
        }
        internal void SaveIbsServers()
        {
            SaveAll(IBSServers);

            ReloadIbsServers();
        }

        public void ReloadIbsServers()
        {
            IBSServers.Clear();
            UseDbContext(context => context.Set<IBSServer>().OrderBy(x => x.Name).ToList().ForEach(IBSServers.Add));
        }

        public void SaveEnvironments()
        {
            SaveAll(TaxiHailEnvironments);
            ReloadEnvironments();

        }

        private void ReloadEnvironments()
        {
            TaxiHailEnvironments.Clear();
            UseDbContext(context => context.Set<TaxiHailEnvironment>().ToList().ForEach(TaxiHailEnvironments.Add));
        }

        public void SaveCompanies()
        {
            SaveAll(Companies);
            ReloadCompanies();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MK.ConfigurationManager.Annotations;
using MK.ConfigurationManager.Entities;

namespace MK.ConfigurationManager.Tabs
{
    public class DeploymentViewModel : INotifyPropertyChanged
    {
        public DeploymentViewModel()
        {
            SelectedJob = ConfigurationManagerDatabase.Current.DeploymentJobs.FirstOrDefault();
        }

        public ObservableCollection<IBSServer> IBSServers { get { return ConfigurationManagerDatabase.Current.IBSServers; } }
        public ObservableCollection<TaxiHailEnvironment> TaxiHailEnvironments { get { return ConfigurationManagerDatabase.Current.TaxiHailEnvironments; } }
        public ObservableCollection<AppVersion> Versions { get { return ConfigurationManagerDatabase.Current.Versions; } }
        public ObservableCollection<AppVersion> VersionsNotHidden { get { return ConfigurationManagerDatabase.Current.VersionsNotHidden; } }
        public ObservableCollection<DeploymentJob> DeploymentJobs { get { return ConfigurationManagerDatabase.Current.DeploymentJobs; } }
        public ObservableCollection<Company> Companies { get { return ConfigurationManagerDatabase.Current.Companies; } }



        public Company DeployCompany { get; set; }
        public IBSServer DeployIBSServer { get; set; }
        public TaxiHailEnvironment DeployTaxiHailEnv { get; set; }
        public AppVersion DeployVersion { get; set; }

        private DeploymentJob _selectedJob;
        public DeploymentJob SelectedJob
        {
            get { return _selectedJob; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _selectedJob = value;

                OnPropertyChanged();


                DeployRevision = _selectedJob.Revision;
                DeployDB = _selectedJob.DeployDB;
                DeployServer = _selectedJob.DeployServer;
                DeployAndroid = _selectedJob.Android;
                DeployIosAdHoc = _selectedJob.iOS_AdHoc;
                DeployIosAppStore = _selectedJob.iOS_AppStore;
                DeployCallBox = _selectedJob.CallBox;
                DeployInitDatabse = _selectedJob.InitDatabase;

            }
        }


        bool _deployInitDatabse;
        public bool DeployInitDatabse
        {
            get { return _deployInitDatabse; }
            set
            {
                _deployInitDatabse = value;
                OnPropertyChanged();
            }
        }
        bool _deployServer;
        public bool DeployServer
        {
            get { return _deployServer; }
            set
            {
                _deployServer = value;
                OnPropertyChanged();
            }
        }
        bool _deployIosAdHoc;
        public bool DeployIosAdHoc
        {
            get { return _deployIosAdHoc; }
            set
            {
                _deployIosAdHoc = value;
                OnPropertyChanged();
            }
        }
        bool _deployIosAppStore;
        public bool DeployIosAppStore
        {
            get { return _deployIosAppStore; }
            set
            {
                _deployIosAppStore = value;
                OnPropertyChanged();
            }
        }
        bool _deployAndroid;
        public bool DeployAndroid
        {
            get { return _deployAndroid; }
            set
            {
                _deployAndroid = value;
                OnPropertyChanged();
            }
        }
        bool _deployCallBox;
        public bool DeployCallBox
        {
            get { return _deployCallBox; }
            set
            {
                _deployCallBox = value;
                OnPropertyChanged();
            }
        }
        bool _deployDB;
        public bool DeployDB
        {
            get { return _deployDB; }
            set
            {
                _deployDB = value;
                OnPropertyChanged();
            }
        }
        public string _deployRevision;
        public string DeployRevision
        {
            get { return _deployRevision; }
            set
            {
                _deployRevision = value;
                OnPropertyChanged();
            }
        }
        


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void StartJob()
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
            ConfigurationManagerDatabase.Current.AddJob(job);
 
        }
    }
}

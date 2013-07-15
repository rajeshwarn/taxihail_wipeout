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
            SelectedJob = ConfigurationDatabase.Current.DeploymentJobs.FirstOrDefault();

            ConfigurationDatabase.Current.Versions.CollectionChanged += (s, e) => OnPropertyChanged("VersionsNotHidden");
        }

        public ObservableCollection<IBSServer> IBSServers { get { return ConfigurationDatabase.Current.IBSServers; } }
        public ObservableCollection<TaxiHailEnvironment> TaxiHailEnvironments { get { return ConfigurationDatabase.Current.TaxiHailEnvironments; } }

        
        public ObservableCollection<DeploymentJob> DeploymentJobs { get { return ConfigurationDatabase.Current.DeploymentJobs; } }
        public ObservableCollection<Company> Companies { get { return ConfigurationDatabase.Current.Companies; } }

        public IEnumerable<AppVersion> VersionsNotHidden
        {
            get { return ConfigurationDatabase.Current.Versions.Where(x => !x.Hidden).Concat(new[] { new AppVersion() }); }
        }




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
                SelectedCompany = Companies.First(c => c.Id == _selectedJob.Company.Id);
                SelectedIBSServer = IBSServers.First(i => i.Id == _selectedJob.IBSServer.Id);
                SelectedEnvironment = TaxiHailEnvironments.First(e =>e.Id == _selectedJob.TaxHailEnv.Id);

                if (_selectedJob.Version != null)
                {
                    SelectedVersion = VersionsNotHidden.FirstOrDefault(v => v.Id == _selectedJob.Version.Id);
                }

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

        Company _selectedCompany;
        public Company SelectedCompany
        {
            get { return _selectedCompany; }
            set
            {
                _selectedCompany = value;
                OnPropertyChanged();
            }
        }


        IBSServer _selectedIBSServer;
        public IBSServer SelectedIBSServer
        {
            get { return _selectedIBSServer; }
            set
            {
                _selectedIBSServer = value;
                OnPropertyChanged();
            }
        }

        TaxiHailEnvironment _selectedEnvironment;
        public TaxiHailEnvironment SelectedEnvironment
        {
            get { return _selectedEnvironment; }
            set
            {
                _selectedEnvironment = value;
                OnPropertyChanged();
            }
        }
        AppVersion _selectedVersion;
        public AppVersion SelectedVersion
        {
            get { return _selectedVersion; }
            set
            {
                _selectedVersion = value;
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
            var job = new DeploymentJob
                {
                    RequestedDate = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Company = SelectedCompany,
                    IBSServer = SelectedIBSServer,
                    Revision = DeployRevision,
                    Version = SelectedVersion,
                    TaxHailEnv = SelectedEnvironment,
                    DeployDB = DeployDB,
                    InitDatabase = DeployInitDatabse,
                    Android = DeployAndroid,
                    CallBox = DeployCallBox,
                    DeployServer = DeployServer,
                    iOS_AdHoc = DeployIosAdHoc,
                    iOS_AppStore = DeployIosAppStore,
                    Status = JobStatus.REQUESTED
                };
            ConfigurationDatabase.Current.AddJob(job);
 
        }
    }
}

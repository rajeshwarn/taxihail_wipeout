using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows;
using MK.ConfigurationManager.Entities;

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

        public ObservableCollection<MyCustomKeyValuePair> ConfigurationProperties { get; set; } 

        public Company CurrentCompany
        {
            get { return _currentCompany; }
            set
            {
                _currentCompany = value;
                OnPropertyChanged("CurrentCompany");
                ConfigurationProperties.Clear(); 
                CurrentCompany.ConfigurationProperties.ToList().ForEach(x => ConfigurationProperties.Add(new MyCustomKeyValuePair(x.Key, x.Value)));
                //if(!ConfigurationProperties.Any())
                //{
                //    ConfigurationProperties.Add(new MyCustomKeyValuePair(null, null));
                //}
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindowLoaded;

            this.DataContext = this;

            Companies = new ObservableCollection<Company>();
            ConfigurationProperties = new ObservableCollection<MyCustomKeyValuePair>();
        }

        void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            //DbContext.Database.CreateIfNotExists();
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ConfigurationManagerDbContext, SimpleDbMigrationsConfiguration>("MKConfig"));
            DbContext = new ConfigurationManagerDbContext(System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString);
            this.currentDbCs.Text = System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString;
            DbContext.Set<Company>().ToList().ForEach(Companies.Add);
        }

        private void addCompabyBt_Click(object sender, RoutedEventArgs e)
        {
            CurrentCompany = new Company() {Id = Guid.NewGuid()};
            DbContext.Set<Company>().Add(CurrentCompany);
            Companies.Add(CurrentCompany);
            ConfigurationProperties.Clear();
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

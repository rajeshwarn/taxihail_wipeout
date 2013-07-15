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
using System.Net;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using MK.ConfigurationManager.Entities;
using MK.ConfigurationManager.Tabs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MK.ConfigurationManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public CompaniesTabViewModel CompaniesTabViewModel { get; set; }
        public DeploymentViewModel DeploymentViewModel { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();

            RepositoryHelper.FetchRepoTags();
            CompaniesTabViewModel = new CompaniesTabViewModel();
            DeploymentViewModel= new DeploymentViewModel();

            DataContext = this;
            
            TabControl.SelectedIndex = 4;

        }


        private void updateCS_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

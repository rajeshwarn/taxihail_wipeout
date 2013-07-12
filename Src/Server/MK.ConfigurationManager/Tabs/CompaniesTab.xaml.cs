using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MK.ConfigurationManager.Annotations;
using MK.ConfigurationManager.Entities;
using Newtonsoft.Json.Linq;
using Path = System.Windows.Shapes.Path;

namespace MK.ConfigurationManager.Tabs
{
    /// <summary>
    /// Interaction logic for CompaniesTab.xaml
    /// </summary>
    public partial class CompaniesTab : UserControl
    {
        public CompaniesTab()
        {
            InitializeComponent();
            DataContext = ConfigurationManagerDatabase.Current.Companies;
        }

        CompaniesTabViewModel ViewModel { get { return (CompaniesTabViewModel)DataContext; } }

        private void GenerateKeyStoreAndMapKey_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GenerateKeyStoreAndMapKey_Click(sender, e);
        }

        private void ImportMobileSettings_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ImportMobileSettings_Click(sender, e);
        }

        private void SaveCompany(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveCompany(sender, e);
        }

        private void addCompabyBt_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.addCompabyBt_Click(sender, e);
        }
    }
}

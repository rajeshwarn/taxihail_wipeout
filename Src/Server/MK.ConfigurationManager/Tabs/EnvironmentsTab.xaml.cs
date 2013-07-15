using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace MK.ConfigurationManager.Tabs
{
    /// <summary>
    /// Interaction logic for EnvironmentsTab.xaml
    /// </summary>
    public partial class EnvironmentsTab : UserControl
    {
        public EnvironmentsTab()
        {
            InitializeComponent();
            DataContext = ConfigurationDatabase.Current.TaxiHailEnvironments;
        }

        private void SaveTaxiHailEnvironment(object sender, RoutedEventArgs e)
        {
            ConfigurationDatabase.Current.DbContext.SaveChanges();
        }
    }
}

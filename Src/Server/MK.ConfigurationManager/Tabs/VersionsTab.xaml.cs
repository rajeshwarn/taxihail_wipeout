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
    /// Interaction logic for VersionsTab.xaml
    /// </summary>
    public partial class VersionsTab : UserControl
    {
        public VersionsTab()
        {
            InitializeComponent();
            DataContext = ConfigurationDatabase.Current.Versions;
        }

        private void SaveVersion(object sender, RoutedEventArgs e)
        {
            ConfigurationDatabase.Current.DbContext.SaveChanges();
        }
    }
}

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
    /// Interaction logic for IbsServersTab.xaml
    /// </summary>
    public partial class IbsServersTab : UserControl
    {
        public IbsServersTab()
        {
            InitializeComponent();
            DataContext = ConfigurationDatabase.Current.IBSServers;
        }

        private void SaveIBSServer(object sender, RoutedEventArgs e)
        {
            ConfigurationDatabase.Current.DbContext.SaveChanges();
        }
    }
}

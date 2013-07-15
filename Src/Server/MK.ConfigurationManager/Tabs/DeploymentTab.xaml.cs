using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
using MK.ConfigurationManager.Entities;

namespace MK.ConfigurationManager.Tabs
{
    /// <summary>
    /// Interaction logic for DeploymentTab.xaml
    /// </summary>
    public partial class DeploymentTab : UserControl
    {
        public DeploymentTab()
        {
            InitializeComponent();
            AutoRefreshCheckbox.Checked += AutoRefreshCheckbox_Checked;



            OutputTextBox.ScrollToEnd();
 

            DeployVersionCombobox.DropDownClosed +=(s, e) =>
                {
                    RevisionTextBox.IsEnabled = string.IsNullOrWhiteSpace(DeployVersionCombobox.Text);
                    if (RevisionTextBox.IsEnabled)
                    {
                        RevisionTextBox.Text = "";
                    }
                };

        }


        private DeploymentViewModel ViewModel
        {
            get { return (DeploymentViewModel) DataContext; }
        }

        void AutoRefreshCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (AutoRefreshCheckbox.IsChecked.HasValue && AutoRefreshCheckbox.IsChecked.Value)
            {
                AutoRefresh();
            }
        }

        private void AutoRefresh()
        {
            Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    RefreshButton.IsEnabled = false;
                });
                Dispatcher.Invoke(() =>
                    {
                        ConfigurationDatabase.Current.ReloadDeployments();
                        OutputTextBox.ScrollToEnd();
                        DeployDataGrid.SelectedIndex = 0;

                        if (AutoRefreshCheckbox.IsChecked.HasValue && AutoRefreshCheckbox.IsChecked.Value)
                        {
                            AutoRefresh();
                        }
                    });
                Dispatcher.Invoke(() =>
                {
                    RefreshButton.IsEnabled = true;
                });
            });
        }

        private void RefreshDeployments(object sender, RoutedEventArgs e)
        {
           ConfigurationDatabase.Current.ReloadDeployments();
        }

        private void ClearDeployHistory(object sender, RoutedEventArgs e)
        {
            ConfigurationDatabase.Current.ClearDeployHistory();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ViewModel.StartJob();
        }


    }
}

using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Windows;

namespace MK.ConfigurationManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConfigurationManagerDbContext DbContext { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Database.SetInitializer<ConfigurationManagerDbContext>(null);
            DbContext = new ConfigurationManagerDbContext(System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString);
            this.currentDbCs.Text =
                System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString;
        }

        private void CreateSchemaBtClick(object sender, RoutedEventArgs e)
        {
            statusBarTb.Text = "Creating Schema";
            var adapter = (IObjectContextAdapter)DbContext;
            var script = adapter.ObjectContext.CreateDatabaseScript();
            DbContext.Database.ExecuteSqlCommand(script);
            statusBarTb.Text = "Done";
        }
    }
}

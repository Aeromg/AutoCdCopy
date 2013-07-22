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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Windows.Shell;
namespace AutoCdCopy
{
    /// <summary>
    /// Interaction logic for MainWindowMetro.xaml
    /// </summary>
    public partial class MainWindowMetro : MetroWindow
    {
        public MainWindowViewModel ViewModel { get; set; }
        public object SelectedDriveItem;

        public MainWindowMetro()
        {
            //ViewModel = new MainWindowViewModel();
            //DataContext = ViewModel;
            InitializeComponent();
            drives.SelectionChanged += drives_Selected_1;
            TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
            App.CurrentProgress = 0;
            App.ProgressChanged += App_ProgressChanged;
        }

        void App_ProgressChanged(object sender, EventArgs e)
        {
            TaskbarItemInfo.ProgressValue = App.CurrentProgress;
            if (App.CurrentProcessesError)
            {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
                return;
            }
            if (App.CurrentProcessesIntermediate)
            {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
                return;
            }

            if (App.CurrentProgress == 0)
            {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                TaskbarItemInfo.Description = "";
            }
            else
            {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                TaskbarItemInfo.Description = String.Format("Copy: {0:0.00}%", App.CurrentProgress * 100);
            }
        }

        private void drives_Selected_1(object sender, RoutedEventArgs e)
        {
            if (drives.SelectedItem == null)
                drives.SelectedItem = SelectedDriveItem;

            SelectedDriveItem = drives.SelectedItem;
        }
    }
}

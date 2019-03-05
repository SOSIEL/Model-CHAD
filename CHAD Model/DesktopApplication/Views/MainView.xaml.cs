using System.Windows;
using DesktopApplication.ViewModels;
using DesktopApplication.ViewServices;

namespace DesktopApplication.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        private readonly INavigationService _navigationService;

        private readonly SimulatorViewModel _simulatorViewModel;

        public MainView()
        {
            InitializeComponent();
        }

        public MainView(INavigationService navigationService, SimulatorViewModel simulatorViewModel)
        {
            _navigationService = navigationService;
            DataContext = _simulatorViewModel = simulatorViewModel;

            InitializeComponent();
        }

        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            _simulatorViewModel.Start();
        }

        private void Stop_OnClick(object sender, RoutedEventArgs e)
        {
            _simulatorViewModel.Stop();
        }

        private void Pause_OnClick(object sender, RoutedEventArgs e)
        {
            _simulatorViewModel.Pause();
        }
    }
}

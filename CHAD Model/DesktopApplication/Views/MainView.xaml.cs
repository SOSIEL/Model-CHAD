using System.Windows;
using DesktopApplication.ViewModels;
using DesktopApplication.ViewServices;

namespace DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        #region Fields

        private readonly INavigationService _navigationService;

        private readonly SimulatorViewModel _simulatorViewModel;

        #endregion

        #region Constructors

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

        #endregion

        #region All other members

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

        private void AddNewSimulation_OnClick(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateToEditSimulationView(_simulatorViewModel.MakeNewSimulationViewModel());
        }

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateToEditSimulationView(_simulatorViewModel.SelectedViewModel);
        }

        #endregion
    }
}
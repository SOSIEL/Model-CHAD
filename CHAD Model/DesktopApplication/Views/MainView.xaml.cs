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

        private readonly ApplicationViewModel _applicationViewModel;

        private readonly INavigationService _navigationService;

        #endregion

        #region Constructors

        public MainView()
        {
            InitializeComponent();
        }

        public MainView(INavigationService navigationService, ApplicationViewModel applicationViewModel)
        {
            _navigationService = navigationService;
            DataContext = _applicationViewModel = applicationViewModel;

            _applicationViewModel.AddDispatcher(Dispatcher);

            InitializeComponent();
        }

        #endregion

        #region All other members

        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            _applicationViewModel.Start();
        }

        private void Stop_OnClick(object sender, RoutedEventArgs e)
        {
            _applicationViewModel.Stop();
        }

        private void Pause_OnClick(object sender, RoutedEventArgs e)
        {
            _applicationViewModel.Pause();
        }

        private void AddNewSimulation_OnClick(object sender, RoutedEventArgs e)
        {
            var configurationEditorViewModel =
                new ConfigurationEditorViewModel(_applicationViewModel.MakeConfigurationViewModel());

            _navigationService.NavigateToParametersView(configurationEditorViewModel);
        }

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            var configurationEditorViewModel =
                new ConfigurationEditorViewModel(_applicationViewModel.ConfigurationViewModel);

            _navigationService.NavigateToParametersView(configurationEditorViewModel);
        }

        #endregion
    }
}
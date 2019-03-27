using System.Windows;
using CHAD.DesktopApplication.ViewModels;
using CHAD.DesktopApplication.ViewServices;

namespace CHAD.DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for NewSimulationView.xaml
    /// </summary>
    public partial class ConfigurationNameView
    {
        #region Fields

        private readonly ApplicationViewModel _applicationViewModel;

        #endregion

        #region Constructors

        public ConfigurationNameView()
        {
            InitializeComponent();
        }

        public ConfigurationNameView(INavigationService navigationService, ApplicationViewModel applicationViewModel,
            ConfigurationEditorViewModel configurationEditorViewModel)
        {
            _applicationViewModel = applicationViewModel;
            NavigationService = navigationService;
            DataContext = ConfigurationEditorViewModel = configurationEditorViewModel;

            InitializeComponent();
        }

        #endregion

        #region Properties, Indexers

        public INavigationService NavigationService { get; }

        public ConfigurationEditorViewModel ConfigurationEditorViewModel { get; }

        #endregion

        #region All other members

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateToParametersView(ConfigurationEditorViewModel);
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            ConfigurationEditorViewModel.Save();

            if (!_applicationViewModel.ConfigurationsViewModels.Contains(ConfigurationEditorViewModel.OriginalValue))
                _applicationViewModel.AddConfigurationViewModel(ConfigurationEditorViewModel.OriginalValue);

            _applicationViewModel.SaveConfiguration(ConfigurationEditorViewModel.OriginalValue);

            NavigationService.NavigateToMainView();
        }

        #endregion
    }
}
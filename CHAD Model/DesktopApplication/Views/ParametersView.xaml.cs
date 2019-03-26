using System.Windows;
using CHAD.DesktopApplication.ViewModels;
using CHAD.DesktopApplication.ViewServices;

namespace CHAD.DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for ParametersView.xaml
    /// </summary>
    public partial class ParametersView
    {
        #region Fields

        private readonly ConfigurationEditorViewModel _configurationEditorViewModel;

        #endregion

        #region Constructors

        public ParametersView(INavigationService navigationService,
            ConfigurationEditorViewModel configurationEditorViewModel)
        {
            NavigationService = navigationService;
            DataContext = _configurationEditorViewModel = configurationEditorViewModel;

            InitializeComponent();
        }

        #endregion

        #region Properties, Indexers

        public INavigationService NavigationService { get; }

        #endregion

        #region All other members

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateToMainView();
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateToConfigurationNameView(_configurationEditorViewModel);
        }

        #endregion
    }
}
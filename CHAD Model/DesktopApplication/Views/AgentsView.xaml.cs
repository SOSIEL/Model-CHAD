using System.Windows;
using DesktopApplication.ViewModels;
using DesktopApplication.ViewServices;

namespace DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for AgentsView.xaml
    /// </summary>
    public partial class AgentsView
    {
        #region Fields

        private readonly ConfigurationEditorViewModel _configurationEditorViewModel;

        #endregion

        #region Constructors

        public AgentsView()
        {
            InitializeComponent();
        }

        public AgentsView(INavigationService navigationService,
            ConfigurationEditorViewModel configurationEditorViewModel)
        {
            NavigationService = navigationService;
            _configurationEditorViewModel = configurationEditorViewModel;

            InitializeComponent();
        }

        #endregion

        #region Properties, Indexers

        public INavigationService NavigationService { get; }

        #endregion

        #region All other members

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateToParametersView(_configurationEditorViewModel);
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateToOutputView(_configurationEditorViewModel);
        }

        #endregion
    }
}
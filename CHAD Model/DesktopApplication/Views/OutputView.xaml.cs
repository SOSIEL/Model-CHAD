using System;
using System.Windows;
using DesktopApplication.ViewModels;
using DesktopApplication.ViewServices;

namespace DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for OutputView.xaml
    /// </summary>
    public partial class OutputView
    {
        #region Fields

        private readonly ConfigurationEditorViewModel _configurationEditorViewModel;

        #endregion

        #region Constructors

        public OutputView()
        {
            InitializeComponent();
        }

        public OutputView(INavigationService navigationService,
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

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateToAgentsView(_configurationEditorViewModel);
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateToConfigurationNameView(_configurationEditorViewModel);
        }
    }
}
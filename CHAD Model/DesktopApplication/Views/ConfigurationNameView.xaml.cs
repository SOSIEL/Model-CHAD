﻿using System.Windows;
using DesktopApplication.ViewModels;
using DesktopApplication.ViewServices;

namespace DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for NewSimulationView.xaml
    /// </summary>
    public partial class ConfigurationNameView
    {
        #region Fields

        private readonly SimulatorViewModel _simulatorViewModel;

        #endregion

        #region Constructors

        public ConfigurationNameView()
        {
            InitializeComponent();
        }

        public ConfigurationNameView(INavigationService navigationService, SimulatorViewModel simulatorViewModel,
            ConfigurationEditorViewModel configurationEditorViewModel)
        {
            _simulatorViewModel = simulatorViewModel;
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
            NavigationService.NavigateToOutputView(ConfigurationEditorViewModel);
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            ConfigurationEditorViewModel.Save();

            if (!_simulatorViewModel.ConfigurationsViewModels.Contains(ConfigurationEditorViewModel.OriginalValue))
                _simulatorViewModel.AddConfigurationViewModel(ConfigurationEditorViewModel.OriginalValue);

            _simulatorViewModel.Configure();

            NavigationService.NavigateToMainView();
        }

        #endregion
    }
}
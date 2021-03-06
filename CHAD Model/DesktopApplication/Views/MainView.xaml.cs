﻿using System.Windows;
using System.Windows.Controls;
using CHAD.DesktopApplication.ViewModels;
using CHAD.DesktopApplication.ViewServices;
using CHAD.Model;

namespace CHAD.DesktopApplication.Views
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

        private void Pause_OnClick(object sender, RoutedEventArgs e)
        {
            _applicationViewModel.Pause();
        }

        private void SosielConfigurationsCombobox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var name = (string) e.AddedItems[0];

            if (_applicationViewModel.ConfigurationViewModel == null ||
                _applicationViewModel.ConfigurationViewModel.SosielConfiguration.Equals(name))
                return;

            _applicationViewModel.ConfigurationViewModel.SosielConfiguration = name;
            _applicationViewModel.SaveConfiguration(_applicationViewModel.ConfigurationViewModel);
            _applicationViewModel.ConfigurationViewModel = _applicationViewModel.ConfigurationViewModel;
        }

        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            if (_applicationViewModel.Simulator.Status == SimulatorStatus.Stopped)
                _applicationViewModel.Start();

            if (_applicationViewModel.Simulator.Status == SimulatorStatus.OnPaused)
                _applicationViewModel.Continue();
        }

        private void Stop_OnClick(object sender, RoutedEventArgs e)
        {
            _applicationViewModel.Stop();
        }

        private void Edit_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (_applicationViewModel.ConfigurationViewModel != null)
                _applicationViewModel.SaveConfiguration(_applicationViewModel.ConfigurationViewModel);
        }

        #endregion
    }
}
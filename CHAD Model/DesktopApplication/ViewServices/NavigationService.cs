using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using CHAD.DesktopApplication.Properties;
using CHAD.DesktopApplication.ViewModels;
using CHAD.DesktopApplication.Views;
using Unity;
using Unity.Resolution;

namespace CHAD.DesktopApplication.ViewServices
{
    public interface INavigationService : INotifyPropertyChanged
    {
        #region Properties, Indexers

        UserControl CurrentView { get; }
        bool CanNavigatePrevious { get; }
        bool CanNavigateNext { get; }
        string NextButtonText { get; }
        string PreviousButtonText { get; }

        #endregion

        #region All other members

        void NavigateToMainView();
        void NavigateToAgentsView(ConfigurationEditorViewModel configurationEditorViewModel);
        void NavigateToParametersView(ConfigurationEditorViewModel configurationEditorViewModel);
        void NavigateToOutputView(ConfigurationEditorViewModel configurationEditorViewModel);
        void NavigateToConfigurationNameView(ConfigurationEditorViewModel configurationEditorViewModel);

        #endregion
    }

    public class NavigationService : INavigationService
    {
        #region Fields

        private readonly ApplicationViewModel _simulatorViewModel;
        private readonly IUnityContainer _unityContainer;
        private UserControl _currentView;

        #endregion

        #region Constructors

        public NavigationService(IUnityContainer unityContainer, ApplicationViewModel simulatorViewModel)
        {
            _unityContainer = unityContainer;
            _simulatorViewModel = simulatorViewModel;

            simulatorViewModel.SimulatorStatusChanged += SimulatorViewModelOnStatusChanged;
        }

        #endregion

        #region Properties, Indexers

        public UserControl CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;

                OnPropertyChanged(nameof(CurrentView));
                OnPropertyChanged(nameof(NextButtonText));
                RaiseCanNavigateChanged();
            }
        }

        public bool CanNavigatePrevious => !(_currentView is MainView);

        public bool CanNavigateNext
        {
            get
            {
                if (CurrentView is MainView)
                    return _simulatorViewModel.ConfigurationViewModel != null;

                return true;
            }
        }

        public string PreviousButtonText => Properties.Resources.PreviousButtonText;

        public string NextButtonText
        {
            get
            {
                switch (CurrentView)
                {
                    case MainView _:
                        return Properties.Resources.Configure;
                    case ConfigurationNameView _:
                        return Properties.Resources.Save;
                    case OutputView _:
                        return Properties.Resources.Finish;
                    default:
                        return Properties.Resources.NextButtonText;
                }
            }
        }

        #endregion

        #region Interface Implementations

        public void NavigateToMainView()
        {
            CurrentView = _unityContainer.Resolve<MainView>();
        }

        public void NavigateToAgentsView(ConfigurationEditorViewModel configurationEditorViewModel)
        {
            CurrentView = _unityContainer.Resolve<AgentsView>(new ParameterOverride("configurationEditorViewModel",
                configurationEditorViewModel));
        }

        public void NavigateToParametersView(ConfigurationEditorViewModel configurationEditorViewModel)
        {
            CurrentView =
                _unityContainer.Resolve<ParametersView>(new ParameterOverride("configurationEditorViewModel",
                    configurationEditorViewModel));
        }

        public void NavigateToOutputView(ConfigurationEditorViewModel configurationEditorViewModel)
        {
            CurrentView = _unityContainer.Resolve<OutputView>(new ParameterOverride("configurationEditorViewModel",
                configurationEditorViewModel));
        }

        public void NavigateToConfigurationNameView(ConfigurationEditorViewModel configurationEditorViewModel)
        {
            CurrentView =
                _unityContainer.Resolve<ConfigurationNameView>(new ParameterOverride("configurationEditorViewModel",
                    configurationEditorViewModel));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region All other members

        private void SimulatorViewModelOnStatusChanged()
        {
            RaiseCanNavigateChanged();
        }

        private void RaiseCanNavigateChanged()
        {
            OnPropertyChanged(nameof(CanNavigatePrevious));
            OnPropertyChanged(nameof(CanNavigateNext));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using DesktopApplication.Annotations;
using DesktopApplication.ViewModels;
using DesktopApplication.Views;
using Unity;
using Unity.Resolution;

namespace DesktopApplication.ViewServices
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
        void NavigateToAgentsView();
        void NavigateToParametersView();
        void NavigateToOutputView();
        void NavigateToEditSimulationView(ConfigurationViewModel simulationViewModel);
        void NavigatePrevious();
        void NavigateNext();

        #endregion
    }

    public class NavigationService : INavigationService
    {
        #region Fields

        private readonly SimulatorViewModel _simulatorViewModel;
        private readonly IUnityContainer _unityContainer;
        private UserControl _currentView;

        #endregion

        #region Constructors

        public NavigationService(IUnityContainer unityContainer, SimulatorViewModel simulatorViewModel)
        {
            _unityContainer = unityContainer;
            _simulatorViewModel = simulatorViewModel;

            simulatorViewModel.StatusChanged += SimulatorViewModelOnStatusChanged;
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
                    return _simulatorViewModel.SelectedViewModel != null;

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
                    case EditSimulationView _:
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

        public void NavigateToAgentsView()
        {
            CurrentView = _unityContainer.Resolve<AgentsView>();
        }

        public void NavigateToParametersView()
        {
            CurrentView = _unityContainer.Resolve<ParametersView>();
        }

        public void NavigateToOutputView()
        {
            CurrentView = _unityContainer.Resolve<OutputView>();
        }

        public void NavigateToEditSimulationView(ConfigurationViewModel simulationViewModel)
        {
            var editor = new ConfigurationEditorViewModel(_simulatorViewModel, simulationViewModel);

            CurrentView = _unityContainer.Resolve<EditSimulationView>(new ParameterOverride("simulationEditorViewModel", editor));
        }

        public void NavigatePrevious()
        {
            switch (CurrentView)
            {
                case ParametersView _:
                    NavigateToMainView();
                    return;
                case AgentsView _:
                    NavigateToParametersView();
                    return;
                case OutputView _:
                    NavigateToAgentsView();
                    return;
                case EditSimulationView _:
                    NavigateToMainView();
                    break;
            }
        }

        public void NavigateNext()
        {
            switch (CurrentView)
            {
                case MainView _:
                    NavigateToParametersView();
                    return;
                case ParametersView _:
                    NavigateToAgentsView();
                    return;
                case AgentsView _:
                    NavigateToOutputView();
                    return;
                case EditSimulationView editSimulationView:
                    editSimulationView.SimulationEditorViewModel.Save();
                    NavigateToMainView();
                    break;
                case OutputView _:
                    _simulatorViewModel.Configure();
                    NavigateToMainView();
                    break;
            }
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
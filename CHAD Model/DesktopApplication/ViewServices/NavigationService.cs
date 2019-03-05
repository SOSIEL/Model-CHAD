using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using DesktopApplication.Annotations;
using DesktopApplication.Views;
using Unity;

namespace DesktopApplication.ViewServices
{
    public interface INavigationService : INotifyPropertyChanged
    {
        UserControl CurrentView { get; }
        bool CanNavigatePrevious { get; }
        bool CanNavigateNext { get; }
        void NavigateToMainView();
        void NavigateToAgentsView();
        void NavigateToParametersView();
        void NavigateToOutputView();
        void NavigateToNewSimulationView();
        void NavigatePrevious();
        void NavigateNext();
    }

    public class NavigationService : INavigationService
    {
        private readonly IUnityContainer _unityContainer;
        private UserControl _currentView;
        private bool _canNavigatePrevious;
        private bool _canNavigateNext;

        public NavigationService(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public UserControl CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;

                CanNavigatePrevious = !(_currentView is MainView);
                CanNavigateNext = !(_currentView is OutputView);

                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public bool CanNavigatePrevious
        {
            get => _canNavigatePrevious;
            private set
            {
                _canNavigatePrevious = value;
                OnPropertyChanged(nameof(CanNavigatePrevious));
            }
        }

        public bool CanNavigateNext
        {
            get => _canNavigateNext;
            private set
            {
                _canNavigateNext = value;
                OnPropertyChanged(nameof(CanNavigateNext));
            }
        }

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

        public void NavigateToNewSimulationView()
        {
            CurrentView = _unityContainer.Resolve<NewSimulationView>();
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
                case NewSimulationView _:
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
                case NewSimulationView _:
                    NavigateToMainView();
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
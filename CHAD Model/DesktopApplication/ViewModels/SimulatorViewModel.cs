using System;
using System.Collections.ObjectModel;
using DesktopApplication.Models;
using DesktopApplication.Services;
using DesktopApplication.Tools;

namespace DesktopApplication.ViewModels
{
    public class SimulatorViewModel : ViewModelBase
    {
        #region Fields

        private readonly IStorageService _storageService;
        private ConfigurationViewModel _selectedViewModel;
        private SimulatorStatus _status;

        #endregion

        #region Constructors

        public SimulatorViewModel(IStorageService storageService)
        {
            _storageService = storageService;

            ConfigurationsViewModels = new ObservableCollection<ConfigurationViewModel>();

            foreach (var simulation in storageService.GetSimulations())
                ConfigurationsViewModels.Add(new ConfigurationViewModel(simulation));
        }

        #endregion

        #region Properties, Indexers

        public SimulatorStatus Status
        {
            get => _status;
            private set
            {
                _status = value;

                RaiseStatusChanged();
            }
        }

        public bool CanStart => (Status == SimulatorStatus.Stopped || Status == SimulatorStatus.OnPaused)
                                && SelectedViewModel != null
                                && SelectedViewModel.IsConfigured;

        public bool CanPause => Status == SimulatorStatus.Run;

        public bool CanStop => Status == SimulatorStatus.Run || Status == SimulatorStatus.OnPaused;

        public ConfigurationViewModel SelectedViewModel
        {
            get => _selectedViewModel;
            set
            {
                _selectedViewModel = value;

                OnPropertyChanged(nameof(SelectedViewModel));
                RaiseStatusChanged();
            }
        }

        public ObservableCollection<ConfigurationViewModel> ConfigurationsViewModels { get; }

        public void AddConfigurationViewModel(ConfigurationViewModel configurationViewModel)
        {
            ConfigurationsViewModels.Add(configurationViewModel);
            SelectedViewModel = configurationViewModel;
        }

        #endregion

        #region All other members

        public event Action StatusChanged;

        private void RaiseStatusChanged()
        {
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanPause));
            OnPropertyChanged(nameof(CanStop));
            StatusChanged?.Invoke();
        }

        public ConfigurationViewModel MakeNewSimulationViewModel()
        {
            return new ConfigurationViewModel(new Configuration());
        }

        public void Start()
        {
            Status = SimulatorStatus.Run;
        }

        public void Stop()
        {
            Status = SimulatorStatus.Stopped;
        }

        public void Pause()
        {
            Status = SimulatorStatus.OnPaused;
        }

        public void Configure()
        {
            SelectedViewModel?.Configure();
            RaiseStatusChanged();
        }

        #endregion
    }
}
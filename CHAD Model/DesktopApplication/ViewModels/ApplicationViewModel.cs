using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using DesktopApplication.Tools;
using Model;

namespace DesktopApplication.ViewModels
{
    public class ApplicationViewModel : ViewModelBase
    {
        #region Fields

        private readonly Timer _checkStatusTimer;

        private readonly IStorageService _storageService;
        private ConfigurationViewModel _configurationViewModel;
        private int _currentDay;
        private int _currentSeason;
        private int _currentSimulation;

        #endregion

        #region Constructors

        public ApplicationViewModel(IStorageService storageService)
        {
            _storageService = storageService;
            Simulator = new Simulator(new FileLoggerFactory());
            Simulator.StatusChanged += SimulatorOnStatusChanged;
            Simulator.SimulationResultObtained += SimulatorOnSimulationResultObtained;

            _checkStatusTimer = new Timer(CheckStatus, null, -1, 100);

            ConfigurationsViewModels = new ObservableCollection<ConfigurationViewModel>();
            foreach (var configuration in storageService.GetConfigurations())
                ConfigurationsViewModels.Add(new ConfigurationViewModel(configuration));
        }

        #endregion

        #region Public Interface

        public Simulator Simulator { get; }

        public event Action SimulatorStatusChanged;

        public ConfigurationViewModel MakeConfigurationViewModel()
        {
            return new ConfigurationViewModel(new Configuration());
        }

        public void Start()
        {
            Task.Run(() => { Simulator.Start(); });
            _checkStatusTimer.Change(0, 100);
        }

        public void Stop()
        {
            Simulator.Stop();
        }

        public void Pause()
        {
            Simulator.Pause();
        }

        public bool IsConfigurationChangingEnabled => Simulator.Status == SimulatorStatus.Stopped;

        public bool IsConfigurationAddingEnabled => Simulator.Status == SimulatorStatus.Stopped;

        public bool IsConfigurationEditingEnabled =>
            ConfigurationViewModel != null && Simulator.Status == SimulatorStatus.Stopped;

        public int CurrentSimulation
        {
            get => _currentSimulation;
            set
            {
                _currentSimulation = value;
                RaisePropertyChangedForDispatchers(nameof(CurrentSimulation));
            }
        }

        public int CurrentSeason
        {
            get => _currentSeason;
            set
            {
                _currentSeason = value;
                RaisePropertyChangedForDispatchers(nameof(CurrentSeason));
            }
        }

        public int CurrentDay
        {
            get => _currentDay;
            set
            {
                _currentDay = value;
                RaisePropertyChangedForDispatchers(nameof(CurrentDay));
            }
        }

        public bool CanStart =>
            (Simulator.Status == SimulatorStatus.Stopped || Simulator.Status == SimulatorStatus.OnPaused)
            && ConfigurationViewModel != null;

        public bool CanPause => Simulator.Status == SimulatorStatus.Run;

        public bool CanStop =>
            Simulator.Status == SimulatorStatus.Run || Simulator.Status == SimulatorStatus.OnPaused;

        public ConfigurationViewModel ConfigurationViewModel
        {
            get => _configurationViewModel;
            set
            {
                if (_configurationViewModel != null)
                    _storageService.SaveConfiguration(_configurationViewModel.Configuration, true);

                _configurationViewModel = value;
                _storageService.GetConfiguration(_configurationViewModel.Configuration);
                Simulator.SetConfiguration(_configurationViewModel.Configuration);

                RaisePropertyChanged(nameof(ConfigurationViewModel));
                RaiseStatusChanged();
            }
        }

        public ObservableCollection<ConfigurationViewModel> ConfigurationsViewModels { get; }

        public void AddConfigurationViewModel(ConfigurationViewModel configurationViewModel)
        {
            ConfigurationsViewModels.Add(configurationViewModel);
            ConfigurationViewModel = configurationViewModel;
        }

        public void SaveConfiguration(ConfigurationViewModel configurationViewModel)
        {
            _storageService.SaveConfiguration(configurationViewModel.Configuration, true);
        }

        #endregion

        #region All other members

        private void SimulatorOnSimulationResultObtained(SimulationResult simulationResult)
        {
            _storageService.SaveSimulationResult(simulationResult);
        }

        private void CheckStatus(object state)
        {
            CurrentSimulation = Simulator.CurrentSimulation;
            CurrentSeason = Simulator.CurrentSeason;
            CurrentDay = Simulator.CurrentDay;
        }

        private void SimulatorOnStatusChanged()
        {
            RaiseStatusChanged();
        }

        private void RaiseStatusChanged()
        {
            RaisePropertyChanged(nameof(CanStart));
            RaisePropertyChanged(nameof(CanPause));
            RaisePropertyChanged(nameof(CanStop));
            RaisePropertyChanged(nameof(IsConfigurationChangingEnabled));
            RaisePropertyChanged(nameof(IsConfigurationAddingEnabled));
            RaisePropertyChanged(nameof(IsConfigurationEditingEnabled));
            SimulatorStatusChanged?.Invoke();
        }

        #endregion
    }
}
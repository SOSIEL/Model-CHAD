using System.Collections.ObjectModel;
using DesktopApplication.Models;
using DesktopApplication.Services;
using DesktopApplication.Tools;

namespace DesktopApplication.ViewModels
{
    public class SimulatorViewModel : ViewModelBase
    {
        private readonly IStorageService _storageService;
        private SimulationViewModel _selectedViewModel;
        private bool _canStart;
        private bool _canPause;
        private bool _canStop;
        private SimulatorStatus _status;

        public SimulatorStatus Status
        {
            get => _status;
            private set
            {
                _status = value;

                switch (value)
                {
                    case SimulatorStatus.Stopped:
                        CanStart = true;
                        CanPause = false;
                        CanStop = false;
                        break;
                    case SimulatorStatus.Run:
                        CanStart = false;
                        CanPause = true;
                        CanStop = true;
                        break;
                    case SimulatorStatus.OnPaused:
                        CanStart = true;
                        CanPause = false;
                        CanStop = true;
                        break;
                }
            }
        }

        public bool CanStart
        {
            get => _canStart;
            private set
            {
                _canStart = value;
                OnPropertyChanged(nameof(CanStart));
            }
        }

        public bool CanPause
        {
            get => _canPause;
            set
            {
                _canPause = value; 
                OnPropertyChanged(nameof(CanPause));
            }
        }

        public bool CanStop
        {
            get => _canStop;
            set
            {
                _canStop = value;
                OnPropertyChanged(nameof(CanStop));
            }
        }

        public SimulatorViewModel(IStorageService storageService)
        {
            _storageService = storageService;

            SimulationViewModels = new ObservableCollection<SimulationViewModel>();

            foreach (var simulation in storageService.GetSimulations())
                SimulationViewModels.Add(new SimulationViewModel(simulation));
        }

        public SimulationViewModel SelectedViewModel
        {
            get => _selectedViewModel;
            set
            {
                _selectedViewModel = value;

                if (_selectedViewModel != null)
                    Configure();

                OnPropertyChanged(nameof(SelectedViewModel));
            }
        }


        public ObservableCollection<SimulationViewModel> SimulationViewModels { get; }

        public SimulationViewModel AddNewSimulation(string name)
        {
            var simulation = new Simulation(name);

            var simulationViewModel = new SimulationViewModel(simulation);

            SimulationViewModels.Add(simulationViewModel);

            return simulationViewModel;
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
            Status = SimulatorStatus.Stopped;
        }
    }
}
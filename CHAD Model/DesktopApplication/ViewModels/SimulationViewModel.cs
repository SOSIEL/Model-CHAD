using DesktopApplication.Models;
using DesktopApplication.Tools;

namespace DesktopApplication.ViewModels
{
    public class SimulationViewModel : ViewModelBase
    {
        private readonly Simulation _simulation;
        private bool _isConfigured;

        public SimulationViewModel(Simulation simulation)
        {
            _simulation = simulation;
        }

        public bool IsConfigured
        {
            get => _isConfigured;
            set
            {
                _isConfigured = value;
                OnPropertyChanged(nameof(IsConfigured));
            }
        }

        public void Configure()
        {
            IsConfigured = true;
        }

        public string Name => _simulation.Name;

        public override string ToString()
        {
            return Name;
        }
    }
}
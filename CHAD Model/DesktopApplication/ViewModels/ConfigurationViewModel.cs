using DesktopApplication.Models;
using DesktopApplication.Tools;

namespace DesktopApplication.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        #region Fields

        private readonly Configuration _simulation;
        private bool _isConfigured;

        #endregion

        #region Constructors

        public ConfigurationViewModel(Configuration simulation)
        {
            _simulation = simulation;
        }

        public ConfigurationViewModel(ConfigurationViewModel simulationViewModel)
        {
            _simulation = new Configuration();
            Name = simulationViewModel.Name;
            IsConfigured = _isConfigured;
        }

        #endregion

        #region Properties, Indexers

        public bool IsConfigured
        {
            get => _isConfigured;
            private set
            {
                _isConfigured = value;
                OnPropertyChanged(nameof(IsConfigured));
            }
        }

        public string Name
        {
            get => _simulation.Name;
            set
            {
                _simulation.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        #endregion

        #region All other members

        public void Configure()
        {
            IsConfigured = true;
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
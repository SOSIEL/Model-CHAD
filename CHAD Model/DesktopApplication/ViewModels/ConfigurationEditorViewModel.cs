using DesktopApplication.Tools;

namespace DesktopApplication.ViewModels
{
    public class ConfigurationEditorViewModel : EditorViewModel<ConfigurationViewModel>
    {
        #region Fields

        private readonly SimulatorViewModel _simulatorViewModel;
        private readonly ConfigurationViewModel _configurationViewModel;

        #endregion

        #region Constructors

        public ConfigurationEditorViewModel(SimulatorViewModel simulatorViewModel, ConfigurationViewModel configurationViewModel)
            : base(new ConfigurationViewModel(configurationViewModel))
        {
            _simulatorViewModel = simulatorViewModel;
            _configurationViewModel = configurationViewModel;
        }

        #endregion

        #region All other members

        public override void Save()
        {
            _configurationViewModel.Name = Value.Name;
            
            if(!_simulatorViewModel.ConfigurationsViewModels.Contains(_configurationViewModel))
                _simulatorViewModel.AddConfigurationViewModel(_configurationViewModel);
        }

        #endregion
    }
}
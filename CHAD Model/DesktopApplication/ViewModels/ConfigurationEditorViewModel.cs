using DesktopApplication.Tools;

namespace DesktopApplication.ViewModels
{
    public class ConfigurationEditorViewModel : EditorViewModel<ConfigurationViewModel>
    {
        #region Constructors

        public ConfigurationEditorViewModel(ConfigurationViewModel configurationViewModel)
            : base(configurationViewModel, new ConfigurationViewModel(configurationViewModel))
        {
            OriginalValue.CopyTo(Value);
        }

        #endregion

        #region All other members

        public override void Save()
        {
            Value.CopyTo(OriginalValue);
        }

        #endregion
    }
}
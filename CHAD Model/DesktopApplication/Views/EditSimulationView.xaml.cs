using DesktopApplication.ViewModels;

namespace DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for NewSimulationView.xaml
    /// </summary>
    public partial class EditSimulationView
    {
        #region Constructors

        public EditSimulationView()
        {
            InitializeComponent();
        }

        public EditSimulationView(ConfigurationEditorViewModel simulationEditorViewModel)
        {
            DataContext = SimulationEditorViewModel = simulationEditorViewModel;

            InitializeComponent();
        }

        #endregion

        #region Properties, Indexers

        public ConfigurationEditorViewModel SimulationEditorViewModel { get; }

        #endregion
    }
}
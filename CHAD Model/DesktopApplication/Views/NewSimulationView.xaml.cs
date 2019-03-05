using DesktopApplication.ViewServices;

namespace DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for NewSimulationView.xaml
    /// </summary>
    public partial class NewSimulationView
    {
        private readonly INavigationService _navigationService;

        public NewSimulationView()
        {
            InitializeComponent();
        }

        public NewSimulationView(INavigationService navigationService)
        {
            _navigationService = navigationService;

            InitializeComponent();
        }
    }
}
using DesktopApplication.ViewServices;

namespace DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for AgentsView.xaml
    /// </summary>
    public partial class AgentsView
    {
        private readonly INavigationService _navigationService;

        public AgentsView()
        {
            InitializeComponent();
        }

        public AgentsView(INavigationService navigationService)
        {
            _navigationService = navigationService;

            InitializeComponent();
        }
    }
}
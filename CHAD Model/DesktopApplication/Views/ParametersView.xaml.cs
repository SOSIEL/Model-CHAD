using DesktopApplication.ViewServices;

namespace DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for ParametersView.xaml
    /// </summary>
    public partial class ParametersView
    {
        private readonly INavigationService _navigationService;

        public ParametersView(INavigationService navigationService)
        {
            _navigationService = navigationService;

            InitializeComponent();
        }
    }
}
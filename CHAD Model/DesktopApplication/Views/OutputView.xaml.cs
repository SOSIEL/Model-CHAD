using DesktopApplication.ViewServices;

namespace DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for OutputView.xaml
    /// </summary>
    public partial class OutputView
    {
        private readonly INavigationService _navigationService;

        public OutputView()
        {
            InitializeComponent();
        }

        public OutputView(INavigationService navigationService)
        {
            _navigationService = navigationService;

            InitializeComponent();
        }
    }
}
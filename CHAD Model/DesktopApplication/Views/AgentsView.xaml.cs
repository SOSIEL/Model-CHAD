using DesktopApplication.ViewServices;

namespace DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for AgentsView.xaml
    /// </summary>
    public partial class AgentsView
    {
        #region Fields

        private readonly INavigationService _navigationService;

        #endregion

        #region Constructors

        public AgentsView()
        {
            InitializeComponent();
        }

        public AgentsView(INavigationService navigationService)
        {
            _navigationService = navigationService;

            InitializeComponent();
        }

        #endregion
    }
}
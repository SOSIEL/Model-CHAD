using DesktopApplication.ViewServices;

namespace DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for OutputView.xaml
    /// </summary>
    public partial class OutputView
    {
        #region Fields

        private readonly INavigationService _navigationService;

        #endregion

        #region Constructors

        public OutputView()
        {
            InitializeComponent();
        }

        public OutputView(INavigationService navigationService)
        {
            _navigationService = navigationService;

            InitializeComponent();
        }

        #endregion
    }
}
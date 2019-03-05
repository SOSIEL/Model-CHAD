using DesktopApplication.ViewServices;

namespace DesktopApplication.Views
{
    /// <summary>
    ///     Interaction logic for ParametersView.xaml
    /// </summary>
    public partial class ParametersView
    {
        #region Fields

        private readonly INavigationService _navigationService;

        #endregion

        #region Constructors

        public ParametersView(INavigationService navigationService)
        {
            _navigationService = navigationService;

            InitializeComponent();
        }

        #endregion
    }
}
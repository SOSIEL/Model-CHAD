using System.Windows;
using DesktopApplication.ViewModels;
using DesktopApplication.ViewServices;

namespace DesktopApplication
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(INavigationService navigationService, SimulatorViewModel simulatorViewModel)
        {
            NavigationService = navigationService;
            DataContext = simulatorViewModel;

            InitializeComponent();
        }

        public INavigationService NavigationService { get; }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateToMainView();
        }

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigatePrevious();
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateNext();
        }
    }
}
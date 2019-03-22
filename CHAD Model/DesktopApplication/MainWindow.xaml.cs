using System;
using System.Windows;
using CHAD.DesktopApplication.Tools;
using CHAD.DesktopApplication.ViewModels;
using CHAD.DesktopApplication.ViewServices;

namespace CHAD.DesktopApplication
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(INavigationService navigationService, ApplicationViewModel simulatorViewModel)
        {
            NavigationService = navigationService;
            DataContext = simulatorViewModel;

            InitializeComponent();
        }

        #endregion

        #region Properties, Indexers

        public INavigationService NavigationService { get; }

        #endregion

        #region All other members

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateToMainView();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            this.RemoveIcon();
        }

        #endregion
    }
}
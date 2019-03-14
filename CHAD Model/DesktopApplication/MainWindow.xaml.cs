using System;
using System.Windows;
using DesktopApplication.Tools;
using DesktopApplication.ViewModels;
using DesktopApplication.ViewServices;

namespace DesktopApplication
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
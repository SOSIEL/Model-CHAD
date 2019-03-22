using System.Windows;
using CHAD.DataAccess;
using CHAD.DesktopApplication.ViewModels;
using CHAD.DesktopApplication.Views;
using CHAD.DesktopApplication.ViewServices;
using CHAD.Model.Infrastructure;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace CHAD.DesktopApplication
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region Fields

        private IUnityContainer _unityContainer;

        #endregion

        #region All other members

        protected override void OnStartup(StartupEventArgs e)
        {
            Configure();

            ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            var mainWindow = _unityContainer.Resolve<MainWindow>();
            mainWindow.Show();
        }

        private void Configure()
        {
            _unityContainer = new UnityContainer();

            // Services
            _unityContainer.RegisterType<IStorageService, FileStorageService>(new ContainerControlledLifetimeManager());

            // View Models
            _unityContainer.RegisterType<ApplicationViewModel>(new ContainerControlledLifetimeManager());

            // View Services
            _unityContainer.RegisterType<INavigationService, NavigationService>(
                new ContainerControlledLifetimeManager());

            // Windows
            _unityContainer.RegisterType<MainWindow>(new ContainerControlledLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>(),
                    _unityContainer.Resolve<ApplicationViewModel>()));

            // Views
            _unityContainer.RegisterType<MainView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>(),
                    _unityContainer.Resolve<ApplicationViewModel>()));

            _unityContainer.RegisterType<ParametersView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>(), null));

            _unityContainer.RegisterType<AgentsView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>(), null));

            _unityContainer.RegisterType<OutputView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>(), null));

            _unityContainer.RegisterType<ConfigurationNameView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>(),
                    _unityContainer.Resolve<ApplicationViewModel>(), null));
        }

        #endregion
    }
}
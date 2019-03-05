using System.Windows;
using DesktopApplication.Services;
using DesktopApplication.ViewModels;
using DesktopApplication.Views;
using DesktopApplication.ViewServices;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace DesktopApplication
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
            _unityContainer.RegisterType<IStorageService, StorageService>(new ContainerControlledLifetimeManager());

            // View Models
            _unityContainer.RegisterType<SimulatorViewModel>(new ContainerControlledLifetimeManager());

            // View Services
            _unityContainer.RegisterType<INavigationService, NavigationService>(
                new ContainerControlledLifetimeManager());

            // Windows
            _unityContainer.RegisterType<MainWindow>(new ContainerControlledLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>(),
                    _unityContainer.Resolve<SimulatorViewModel>()));

            // Views
            _unityContainer.RegisterType<MainView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>(),
                    _unityContainer.Resolve<SimulatorViewModel>()));
            _unityContainer.RegisterType<ParametersView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>()));
            _unityContainer.RegisterType<AgentsView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>()));
            _unityContainer.RegisterType<OutputView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>()));
            _unityContainer.RegisterType<EditSimulationView>(new PerResolveLifetimeManager(),
                new InjectionConstructor((ConfigurationEditorViewModel)null));
        }

        #endregion
    }
}
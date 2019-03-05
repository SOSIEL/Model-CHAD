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
        private IUnityContainer _unityContainer;

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
            _unityContainer.RegisterType<IStorageService, StorageService>(new TransientLifetimeManager());

            // View Models
            _unityContainer.RegisterType<SimulatorViewModel>(new TransientLifetimeManager());

            // View Services
            _unityContainer.RegisterType<INavigationService, NavigationService>(new TransientLifetimeManager());

            // Windows
            _unityContainer.RegisterType<MainWindow>(new TransientLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>(), _unityContainer.Resolve<SimulatorViewModel>()));

            // Views
            _unityContainer.RegisterType<MainView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>(), _unityContainer.Resolve<SimulatorViewModel>()));
            _unityContainer.RegisterType<ParametersView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>()));
            _unityContainer.RegisterType<AgentsView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>()));
            _unityContainer.RegisterType<OutputView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>()));
            _unityContainer.RegisterType<NewSimulationView>(new PerResolveLifetimeManager(),
                new InjectionConstructor(_unityContainer.Resolve<INavigationService>()));
        }
    }
}
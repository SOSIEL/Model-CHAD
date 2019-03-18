using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using DesktopApplication.Annotations;

namespace DesktopApplication.Tools
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Fields

        private readonly List<Dispatcher> _dispatchers;

        #endregion

        #region Constructors

        public ViewModelBase()
        {
            _dispatchers = new List<Dispatcher>();
        }

        #endregion

        #region Public Interface

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddDispatcher(Dispatcher dispatcher)
        {
            _dispatchers.Add(dispatcher);
        }

        #endregion

        #region All other members

        [NotifyPropertyChangedInvocator]
        protected void RaisePropertyChangedForDispatchers([CallerMemberName] string propertyName = null)
        {
            foreach (var dispatcher in _dispatchers)
                dispatcher.InvokeAsync(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                });
        }

        [NotifyPropertyChangedInvocator]
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
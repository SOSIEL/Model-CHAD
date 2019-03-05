using System.ComponentModel;
using System.Runtime.CompilerServices;
using DesktopApplication.Annotations;

namespace DesktopApplication.Tools
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Interface Implementations

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region All other members

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
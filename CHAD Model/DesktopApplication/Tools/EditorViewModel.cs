namespace DesktopApplication.Tools
{
    public abstract class EditorViewModel<T> : ViewModelBase
    {
        protected EditorViewModel(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public abstract void Save();
    }
}

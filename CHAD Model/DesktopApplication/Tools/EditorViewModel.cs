namespace DesktopApplication.Tools
{
    public abstract class EditorViewModel<T> : ViewModelBase
    {
        protected EditorViewModel(T originalValue, T value)
        {
            OriginalValue = originalValue;
            Value = value;
        }

        public T OriginalValue { get; }

        public T Value { get; }

        public abstract void Save();
    }
}

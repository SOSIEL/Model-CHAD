namespace SOSIEL.Algorithm
{
    public interface IAlgorithm<TData>
    {
        string Name { get; }

        void Initialize();

        TData Run(TData data);
    }
}

namespace LeanCode.CQRS
{
    public interface IInitializeFromAppContext<TAppContext>
    {
        void Initialize(TAppContext appContext);
    }
}

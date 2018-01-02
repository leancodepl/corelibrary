namespace LeanCode.CQRS.Execution
{
    public interface IObjectContextFromAppContextFactory<TAppContext, TObjContext>
    {
        TObjContext Create(TAppContext appContext);
    }
}

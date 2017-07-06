namespace LeanCode.DomainModels.EventsExecutor
{
    public sealed class Unit
    {
        internal static readonly Unit Instance = new Unit();
        private Unit() { }
    }
}

namespace LeanCode.DomainModels.EventsExecution.Simple
{
    sealed class Unit
    {
        public static readonly Unit Instance = new Unit();

        private Unit() { }
    }
}

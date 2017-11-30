namespace LeanCode.CQRS
{
    public class VoidContext
    {
        public static VoidContext Instance = new VoidContext();
        public static VoidContext Get() => Instance;

        private VoidContext() { }
    }
}

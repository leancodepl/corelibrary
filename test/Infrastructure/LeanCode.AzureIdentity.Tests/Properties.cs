using Xunit;

// This is necessary, because some tests rely on setting environmental variables, which might be overwritten by tests.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

// This is necessary, because some tests rely on setting environmental variables, which might be overwritten by tests.
[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]

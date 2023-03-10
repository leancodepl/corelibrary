using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.EF.Tests;

[SuppressMessage("?", "EF1001", Justification = "Tests.")]
internal sealed class ModelConfigurationBuilderWrapper : ModelConfigurationBuilder
{
    public ModelConfigurationBuilderWrapper()
        : base(new(), new ServiceCollection().BuildServiceProvider()) { }

    public ModelConfiguration Build() => ModelConfiguration;
}

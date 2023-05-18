using Autofac;
using FluentValidation;
using LeanCode.Components;

namespace LeanCode.CQRS.Validation.Fluent.Scoped;

public class FluentValidationModule : AppModule
{
    private readonly TypesCatalog catalog;

    public FluentValidationModule(TypesCatalog catalog)
    {
        this.catalog = catalog;
    }

    protected override void Load(ContainerBuilder builder)
    {

    }
}

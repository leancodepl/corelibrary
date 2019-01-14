using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using LeanCode.Components;
using Xunit;

namespace LeanCode.CQRS.Validation.Fluent.Tests
{
    public class ModuleTests
    {
        private readonly IContainer container;

        public ModuleTests()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new FluentValidationModule(TypesCatalog.Of<ModuleTests>()));
            container = builder.Build();
        }

        [Fact]
        public async Task Resolves_custom_ContextualValidator_as_ICommandValidator()
        {
            var validator = container.Resolve<ICommandValidator<object, CustomCommand>>();

            var res = await validator.ValidateAsync(new object(), new CustomCommand { Field = 0 });

            var err = Assert.Single(res.Errors);
            Assert.Equal(10, err.ErrorCode);
        }
    }

    public class CustomCommand : ICommand
    {
        public int Field { get; set; }
    }

    public class CustomValidator : ContextualValidator<CustomCommand>
    {
        public CustomValidator()
        {
            RuleFor(c => c.Field)
                .GreaterThan(5)
                    .WithCode(10);
        }
    }
}

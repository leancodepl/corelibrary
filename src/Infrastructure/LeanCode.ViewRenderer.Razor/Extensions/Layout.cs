using Microsoft.AspNetCore.Razor.Language;

namespace LeanCode.ViewRenderer.Razor.Extensions
{
    internal class Layout
    {
        public const string DirectiveName = "layout";

        public static readonly DirectiveDescriptor Directive = DirectiveDescriptor.CreateSingleLineDirective(
            DirectiveName, b => b.AddMemberToken());
    }
}

using System;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace LeanCode.ViewRenderer.Razor.Extensions;

internal class LayoutNode : ExtensionIntermediateNode
{
    public const string LayoutFieldName = "___Layout";

    public override IntermediateNodeCollection Children => IntermediateNodeCollection.ReadOnly;

    public string? LayoutName { get; }

    public LayoutNode(string? layoutName)
    {
        LayoutName = layoutName;
    }

    public override void Accept(IntermediateNodeVisitor visitor) => AcceptExtensionNode(this, visitor);

    public override void WriteNode(CodeTarget target, CodeRenderingContext context)
    {
        if (string.IsNullOrEmpty(LayoutName))
        {
            context.CodeWriter.Write("public static readonly string ").Write(LayoutFieldName).WriteLine(" = null;");
        }
        else
        {
            context
                .CodeWriter.Write("public static readonly string ")
                .Write(LayoutFieldName)
                .Write(" = \"")
                .Write(LayoutName)
                .WriteLine("\";");
        }
    }
}

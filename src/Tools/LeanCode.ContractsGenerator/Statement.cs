using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LeanCode.ContractsGenerator
{
    interface IDeclarationStatement
    {
        void Render(StringBuilder builder, int level);
    }

    interface IClientStatement
    {
        void Render(StringBuilder builder, int level);
    }

    static class Spaces
    {
        public static void AppendSpaces(this StringBuilder builder, int level)
        {
            builder.Append(string.Join("", Enumerable.Repeat("    ", level)));
        }
    }

    class FieldStatement : IDeclarationStatement
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsOptional { get; set; } = false;

        public void Render(StringBuilder builder, int level)
        {
            builder.AppendSpaces(level);
            builder.Append(Name);
            builder.Append(": ");
            builder.Append(Type);
            builder.AppendLine(";");
        }
    }

    class InterfaceStatement : IDeclarationStatement
    {
        public string Name { get; set; } = string.Empty;
        public bool IsStatic { get; set; } = false;
        public List<string> Arguments { get; set; } = new List<string>();
        public List<string> Extends { get; set; } = new List<string>();
        public List<FieldStatement> Fields { get; set; } = new List<FieldStatement>();
        public List<ConstStatement> Constants { get; set; } = new List<ConstStatement>();
        public List<InterfaceStatement> Children { get; set; } = new List<InterfaceStatement>();

        public void Render(StringBuilder builder, int level)
        {
            if (!IsStatic)
            {
                builder.AppendSpaces(level);

                builder.Append("interface ");
                builder.Append(Name);

                if (Arguments.Any())
                {
                    builder.Append("<");
                    builder.Append(string.Join(", ", Arguments));
                    builder.Append(">");
                }

                if (Extends.Any())
                {
                    builder.Append(" extends ");

                    builder.Append(string.Join(", ", Extends));
                }

                builder.AppendLine(" {");

                foreach (var field in Fields)
                {
                    field.Render(builder, level + 1);
                }

                builder.AppendSpaces(level);
                builder.AppendLine("}");
                builder.AppendLine();
            }

            if (Children.Any() || Constants.Any())
            {
                builder.AppendSpaces(level);

                builder.Append("namespace ");
                builder.Append(Name);
                builder.AppendLine(" {");

                foreach (var child in Children.Concat<IDeclarationStatement>(Constants))
                {
                    child.Render(builder, level + 1);
                }

                builder.AppendSpaces(level);
                builder.AppendLine("}");
                builder.AppendLine();
            }
        }
    }

    class ConstStatement : IDeclarationStatement
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public void Render(StringBuilder builder, int level)
        {
            builder.AppendSpaces(level);
            builder.Append("const ");
            builder.Append(Name);

            if (!string.IsNullOrWhiteSpace(Value))
            {
                builder.Append(" = ");
                builder.Append(Value);
            }

            builder.AppendLine(";");
        }
    }

    class EnumValueStatement : IDeclarationStatement
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public void Render(StringBuilder builder, int level)
        {
            builder.AppendSpaces(level);
            builder.Append(Name);

            if (!string.IsNullOrWhiteSpace(Value))
            {
                builder.Append(" = ");
                builder.Append(Value);
            }

            builder.AppendLine(",");
        }
    }

    class EnumStatement : IDeclarationStatement
    {
        public string Name { get; set; } = string.Empty;
        public List<EnumValueStatement> Values { get; set; } = new List<EnumValueStatement>();

        public void Render(StringBuilder builder, int level)
        {
            builder.AppendSpaces(level);

            builder.Append("const enum ");
            builder.Append(Name);
            builder.AppendLine(" {");

            foreach (var value in Values)
            {
                value.Render(builder, level + 1);
            }

            builder.AppendSpaces(level);
            builder.AppendLine("}");
            builder.AppendLine();
        }
    }

    class NamespaceStatement : IDeclarationStatement
    {
        public string Name { get; set; } = string.Empty;
        public List<IDeclarationStatement> Children { get; set; } = new List<IDeclarationStatement>();

        public void Render(StringBuilder builder, int level)
        {
            builder.AppendSpaces(level);

            builder.Append("declare namespace ");
            builder.Append(Name);
            builder.AppendLine(" {");

            foreach (var child in Children)
            {
                child.Render(builder, level + 1);
            }

            builder.AppendSpaces(level);
            builder.AppendLine("}");
        }
    }
}

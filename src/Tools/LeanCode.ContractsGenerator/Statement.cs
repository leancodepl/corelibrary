using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LeanCode.ContractsGenerator
{
    interface IDeclarationStatement
    {
        void RenderDeclaration(StringBuilder builder, int level);
    }

    interface IClientStatement
    {
        void RenderClient(StringBuilder paramsBuilder, StringBuilder creatorsBuilder, StringBuilder constsBuilder, int level, string parentName);
    }

    interface IStatement : IDeclarationStatement, IClientStatement
    { }

    static class Spaces
    {
        public static StringBuilder AppendSpaces(this StringBuilder builder, int level)
        {
            return builder.Append(string.Join("", Enumerable.Repeat("    ", level)));
        }
    }

    class FieldStatement : IDeclarationStatement
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsOptional { get; set; } = false;

        public void RenderDeclaration(StringBuilder builder, int level)
        {
            builder.AppendSpaces(level)
                .Append(Name);

            if (IsOptional)
            {
                builder.Append("?");
            }

            builder.Append(": ")
                .Append(Type);

            if (IsOptional)
            {
                builder.Append(" | null");
            }

            builder.AppendLine(";");
        }
    }

    class CommandStatement : InterfaceStatement
    {
        public string NamespaceName { get; set; } = string.Empty;

        public CommandStatement() { }
        public CommandStatement(InterfaceStatement interfaceStatement)
        {
            Name = interfaceStatement.Name;
            IsStatic = interfaceStatement.IsStatic;
            Arguments = interfaceStatement.Arguments;
            Extends = interfaceStatement.Extends;
            Fields = interfaceStatement.Fields;
            Constants = interfaceStatement.Constants;
            Children = interfaceStatement.Children;
        }

        public override void RenderClient(StringBuilder paramsBuilder, StringBuilder creatorsBuilder, StringBuilder constsBuilder, int level = 0, string parentName = null)
        {
            base.RenderClient(paramsBuilder, creatorsBuilder, constsBuilder, level);

            var name = Char.ToLower(Name[0]) + Name.Substring(1);

            paramsBuilder.AppendSpaces(1)
                .Append("\"")
                .Append(name)
                .Append("\": ")
                .Append(parentName)
                .Append(".")
                .Append(Name)
                .AppendLine(",");

            creatorsBuilder.AppendSpaces(2)
                .Append(name)
                .Append(": ")
                .Append("cqrsClient.executeCommand.bind(cqrsClient, \"")
                .Append(NamespaceName)
                .Append(".")
                .Append(Name)
                .AppendLine("\"),");
        }
    }

    class QueryStatement : InterfaceStatement
    {
        public string NamespaceName { get; set; } = string.Empty;

        public QueryStatement() { }
        public QueryStatement(InterfaceStatement interfaceStatement)
        {
            Name = interfaceStatement.Name;
            IsStatic = interfaceStatement.IsStatic;
            Arguments = interfaceStatement.Arguments;
            Extends = interfaceStatement.Extends;
            Fields = interfaceStatement.Fields;
            Constants = interfaceStatement.Constants;
            Children = interfaceStatement.Children;
        }

        public override void RenderClient(StringBuilder paramsBuilder, StringBuilder creatorsBuilder, StringBuilder constsBuilder, int level = 0, string parentName = null)
        {
            base.RenderClient(paramsBuilder, creatorsBuilder, constsBuilder, level);

            var name = Char.ToLower(Name[0]) + Name.Substring(1);

            paramsBuilder.AppendSpaces(1)
                .Append("\"")
                .Append(name)
                .Append("\": ")
                .Append(parentName)
                .Append(".")
                .Append(Name)
                .AppendLine(",");

            creatorsBuilder.AppendSpaces(2)
                .Append(name)
                .Append(": cqrsClient.executeQuery.bind(cqrsClient, \"")
                .Append(NamespaceName)
                .Append(".")
                .Append(Name)
                .AppendLine("\"),");
        }
    }

    class InterfaceStatement : IStatement
    {
        public string Name { get; set; } = string.Empty;
        public bool IsStatic { get; set; } = false;
        public List<string> Arguments { get; set; } = new List<string>();
        public List<string> Extends { get; set; } = new List<string>();
        public List<FieldStatement> Fields { get; set; } = new List<FieldStatement>();
        public List<ConstStatement> Constants { get; set; } = new List<ConstStatement>();
        public List<InterfaceStatement> Children { get; set; } = new List<InterfaceStatement>();

        public void RenderDeclaration(StringBuilder builder, int level)
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
                    field.RenderDeclaration(builder, level + 1);
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

                foreach (var child in Children.Concat<IStatement>(Constants))
                {
                    child.RenderDeclaration(builder, level + 1);
                }

                builder.AppendSpaces(level);
                builder.AppendLine("}");
                builder.AppendLine();
            }
        }

        public virtual void RenderClient(StringBuilder paramsBuilder, StringBuilder creatorsBuilder, StringBuilder constsBuilder, int level = 0, string parentName = null)
        {
            var name = parentName + "." + Name;

            var localConstsBuilder = new StringBuilder();

            foreach (var child in Children)
            {
                child.RenderClient(paramsBuilder, creatorsBuilder, localConstsBuilder, level + 1, name);
            }

            foreach (var constant in Constants)
            {
                constant.RenderClient(paramsBuilder, creatorsBuilder, localConstsBuilder, level + 1, name);
            }

            if (localConstsBuilder.Length > 0 || Constants.Count > 0)
            {
                constsBuilder.AppendSpaces(level);
                constsBuilder.Append(Name);
                constsBuilder.AppendLine(": {");

                constsBuilder.Append(localConstsBuilder);

                constsBuilder.AppendSpaces(level);
                constsBuilder.AppendLine("},");
            }
        }
    }

    class ConstStatement : IStatement
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public void RenderDeclaration(StringBuilder builder, int level)
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

        public void RenderClient(StringBuilder paramsBuilder, StringBuilder creatorsBuilder, StringBuilder constsBuilder, int level, string parentName)
        {
            constsBuilder.AppendSpaces(level);
            constsBuilder.Append(Name);

            if (!string.IsNullOrWhiteSpace(Value))
            {
                constsBuilder.Append(": ");
                constsBuilder.Append(Value);
            }

            constsBuilder.AppendLine(",");
        }
    }

    class EnumValueStatement : IStatement
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public void RenderDeclaration(StringBuilder builder, int level)
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

        public void RenderClient(StringBuilder paramsBuilder, StringBuilder creatorsBuilder, StringBuilder constsBuilder, int level, string parentName)
        {
            constsBuilder.AppendSpaces(level);
            constsBuilder.Append(Name);

            if (!string.IsNullOrWhiteSpace(Value))
            {
                constsBuilder.Append(": ");
                constsBuilder.Append(Value);
            }

            constsBuilder.AppendLine(",");
        }
    }

    class EnumStatement : IStatement
    {
        public string Name { get; set; } = string.Empty;
        public List<EnumValueStatement> Values { get; set; } = new List<EnumValueStatement>();

        public void RenderDeclaration(StringBuilder builder, int level)
        {
            builder.AppendSpaces(level);

            builder.Append("const enum ");
            builder.Append(Name);
            builder.AppendLine(" {");

            foreach (var value in Values)
            {
                value.RenderDeclaration(builder, level + 1);
            }

            builder.AppendSpaces(level);
            builder.AppendLine("}");
            builder.AppendLine();
        }

        public void RenderClient(StringBuilder paramsBuilder, StringBuilder creatorsBuilder, StringBuilder constsBuilder, int level, string parentName)
        {
            constsBuilder.AppendSpaces(level);
            constsBuilder.Append(Name);
            constsBuilder.AppendLine(": {");

            foreach (var value in Values)
            {
                value.RenderClient(paramsBuilder, creatorsBuilder, constsBuilder, level + 1, parentName);
            }

            constsBuilder.AppendSpaces(level);
            constsBuilder.AppendLine("},");
        }
    }

    class ClientStatement : IStatement
    {
        public List<IStatement> Children { get; set; } = new List<IStatement>();
        public string Name { get; set; } = string.Empty;

        public void RenderClient(StringBuilder paramsBuilder, StringBuilder creatorsBuilder, StringBuilder constsBuilder, int level, string parentName)
        {
            paramsBuilder.AppendLine("export type ClientParams = {");

            creatorsBuilder.AppendLine("export default function (cqrsClient: CQRS): ClientType<ClientParams> {");
            creatorsBuilder.AppendLine("    return {");

            var localConstsBuilder = new StringBuilder();

            foreach (var statement in Children)
            {
                statement.RenderClient(paramsBuilder, creatorsBuilder, localConstsBuilder, 1, parentName);
            }

            if (localConstsBuilder.Length > 0)
            {
                constsBuilder.AppendLine($"export const globals: typeof {Name} = {{");
                constsBuilder.Append(localConstsBuilder);
                constsBuilder.AppendLine("};");
            }

            creatorsBuilder.AppendLine("    };");
            creatorsBuilder.AppendLine("}");

            paramsBuilder.AppendLine("};");
        }

        public void Render(StringBuilder declaratiosBuilder, StringBuilder clientBuilder)
        {
            var paramsBuilder = new StringBuilder();
            var creatorsBuilder = new StringBuilder();
            var constsBuilder = new StringBuilder();

            RenderClient(paramsBuilder, creatorsBuilder, constsBuilder, 0, Name);

            clientBuilder.Append(constsBuilder);
            clientBuilder.AppendLine();
            clientBuilder.Append(paramsBuilder);
            clientBuilder.AppendLine();
            clientBuilder.Append(creatorsBuilder);

            RenderDeclaration(declaratiosBuilder, 0);
        }

        public void RenderDeclaration(StringBuilder builder, int level)
        {
            builder.AppendSpaces(level);

            builder.Append("declare namespace ");
            builder.Append(Name);
            builder.AppendLine(" {");

            foreach (var child in Children)
            {
                child.RenderDeclaration(builder, level + 1);
            }

            builder.AppendSpaces(level);
            builder.AppendLine("}");
        }
    }
}


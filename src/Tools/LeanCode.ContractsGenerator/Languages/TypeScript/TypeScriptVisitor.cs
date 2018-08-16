using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LeanCode.ContractsGenerator.Extensions;
using LeanCode.ContractsGenerator.Statements;

namespace LeanCode.ContractsGenerator.Languages.TypeScript
{
    class TypeScriptVisitor : ILanguageVisitor
    {
        private readonly StringBuilder definitionsBuilder;
        private readonly StringBuilder paramsBuilder;
        private readonly StringBuilder creatorsBuilder;
        private readonly StringBuilder constsBuilder;
        private readonly TypeScriptConfiguration configuration;

        public TypeScriptVisitor(TypeScriptConfiguration configuration)
        {
            this.configuration = configuration;

            definitionsBuilder = new StringBuilder();
            paramsBuilder = new StringBuilder();
            creatorsBuilder = new StringBuilder();
            constsBuilder = new StringBuilder();
        }

        public IEnumerable<LanguageFileOutput> Visit(ClientStatement statement)
        {
            definitionsBuilder.Append(configuration.ContractsPreamble).AppendLine();
            constsBuilder.Append(configuration.ClientPreamble).AppendLine();

            Visit(statement, 0, null);

            yield return new LanguageFileOutput
            {
                Name = statement.Name + ".d.ts",
                Content = definitionsBuilder.ToString()
            };

            yield return new LanguageFileOutput
            {
                Name = statement.Name + "Client.ts",
                Content = constsBuilder.Append(paramsBuilder).Append(creatorsBuilder).ToString()
            };
        }

        private void Visit(IStatement statement, int level, string parentName)
        {
            switch (statement)
            {
                case ClientStatement stmt: VisitClientStatement(stmt, level); break;
                case EnumStatement stmt: VisitEnumStatement(stmt, level); break;
                case ConstStatement stmt: VisitConstStatement(stmt, level); break;

                case CommandStatement stmt: VisitCommandStatement(stmt, level, parentName); break;
                case QueryStatement stmt: VisitQueryStatement(stmt, level, parentName); break;

                case InterfaceStatement stmt: VisitInterfaceStatement(stmt, level, parentName); break;
            }
        }

        private void VisitClientStatement(ClientStatement statement, int level)
        {
            paramsBuilder.AppendLine("export type ClientParams = {");

            creatorsBuilder.AppendLine("export default function (cqrsClient: CQRS): ClientType<ClientParams> {");
            creatorsBuilder.AppendLine("    return {");

            definitionsBuilder.AppendSpaces(level);
            definitionsBuilder.Append("declare namespace ");
            definitionsBuilder.Append(statement.Name);
            definitionsBuilder.AppendLine(" {");

            constsBuilder.AppendLine($"export const globals: typeof {statement.Name} = {{");

            foreach (var child in statement.Children)
            {
                Visit(child, level + 1, statement.Name);
            }

            constsBuilder.AppendLine("};").AppendLine();

            definitionsBuilder.AppendSpaces(level);
            definitionsBuilder.AppendLine("}");

            paramsBuilder.AppendLine("};").AppendLine();

            creatorsBuilder.AppendLine("    };");
            creatorsBuilder.AppendLine("}");
        }

        private void VisitEnumStatement(EnumStatement statement, int level)
        {
            definitionsBuilder.AppendSpaces(level)
                .Append("const enum ")
                .Append(statement.Name)
                .AppendLine(" {");

            constsBuilder.AppendSpaces(level)
                .Append(statement.Name)
                .AppendLine(": {");

            foreach (var value in statement.Values)
            {
                VisitEnumValueStatement(value, level + 1);
            }

            definitionsBuilder.AppendSpaces(level)
                .AppendLine("}")
                .AppendLine();

            constsBuilder.AppendSpaces(level)
                .AppendLine("},");
        }

        private void VisitEnumValueStatement(EnumValueStatement statement, int level)
        {
            definitionsBuilder.AppendSpaces(level)
                .Append(statement.Name);

            constsBuilder.AppendSpaces(level)
                .Append(statement.Name);

            if (!string.IsNullOrWhiteSpace(statement.Value))
            {
                definitionsBuilder.Append(" = ").Append(statement.Value);

                constsBuilder.Append(": ").Append(statement.Value);
            }

            definitionsBuilder.AppendLine(",");
            constsBuilder.AppendLine(",");
        }

        private void VisitTypeStatement(TypeStatement statement)
        {
            if (statement.IsDictionary)
            {
                definitionsBuilder.Append("{ [index: ");

                VisitTypeStatement(statement.TypeArguments.First());

                definitionsBuilder.Append("]: ");

                VisitTypeStatement(statement.TypeArguments.Last());

                definitionsBuilder.Append("}");
            }
            else if (statement.IsArrayLike)
            {
                VisitTypeStatement(statement.TypeArguments.First());

                definitionsBuilder.Append("[]");
            }
            else if (statement.TypeArguments.Count > 0)
            {
                definitionsBuilder.Append(statement.Name);
                definitionsBuilder.Append("<");

                for (int i = 0; i < statement.TypeArguments.Count; i++)
                {
                    VisitTypeStatement(statement.TypeArguments[i]);

                    if (i < statement.TypeArguments.Count - 1)
                    {
                        definitionsBuilder.Append(", ");
                    }
                }

                definitionsBuilder.Append(">");
            }
            else if (configuration.TypeTranslations.TryGetValue(statement.Name.ToLower(), out string name))
            {
                definitionsBuilder.Append(name);
            }
            else
            {
                definitionsBuilder.Append(statement.Name);
            }
        }

        private void VisitConstStatement(ConstStatement statement, int level)
        {
            definitionsBuilder.AppendSpaces(level)
                .Append("const ")
                .Append(statement.Name);

            constsBuilder.AppendSpaces(level)
                .Append(statement.Name);

            if (!string.IsNullOrWhiteSpace(statement.Value))
            {
                definitionsBuilder.Append(" = ").Append(statement.Value);

                constsBuilder.Append(": ").Append(statement.Value);
            }

            constsBuilder.AppendLine(",");

            definitionsBuilder.AppendLine(";");
        }

        private void VisitTypeParameterStatement(TypeParameterStatement statement)
        {
            definitionsBuilder.Append(statement.Name);

            if (statement.Constraints.Any())
            {
                definitionsBuilder.Append(" extends ");

                for (int i = 0; i < statement.Constraints.Count; i++)
                {
                    VisitTypeStatement(statement.Constraints[i]);

                    if (i < statement.Constraints.Count - 1)
                    {
                        definitionsBuilder.Append(" & ");
                    }
                }
            }
        }

        private void VisitFieldStatement(FieldStatement statement, int level)
        {
            definitionsBuilder.AppendSpaces(level)
                .Append(statement.Name);

            if (statement.Type.IsNullable)
            {
                definitionsBuilder.Append("?");
            }

            definitionsBuilder.Append(": ");

            VisitTypeStatement(statement.Type);

            if (statement.Type.IsNullable)
            {
                definitionsBuilder.Append(" | null");
            }

            definitionsBuilder.AppendLine(";");
        }

        private void VisitCommandStatement(CommandStatement statement, int level, string parentName)
        {
            VisitInterfaceStatement(statement, level, parentName);

            var name = Char.ToLower(statement.Name[0]) + statement.Name.Substring(1);

            paramsBuilder.AppendSpaces(1)
                .Append("\"")
                .Append(name)
                .Append("\": ")
                .Append(parentName)
                .Append(".")
                .Append(statement.Name)
                .AppendLine(",");

            creatorsBuilder.AppendSpaces(2)
                .Append(name)
                .Append(": ")
                .Append("cqrsClient.executeCommand.bind(cqrsClient, \"")
                .Append(statement.NamespaceName)
                .Append(".")
                .Append(statement.Name)
                .AppendLine("\"),");
        }

        private void VisitQueryStatement(QueryStatement statement, int level, string parentName)
        {
            VisitInterfaceStatement(statement, level, parentName);

            var name = Char.ToLower(statement.Name[0]) + statement.Name.Substring(1);

            paramsBuilder.AppendSpaces(1)
                .Append("\"")
                .Append(name)
                .Append("\": ")
                .Append(parentName)
                .Append(".")
                .Append(statement.Name)
                .AppendLine(",");

            creatorsBuilder.AppendSpaces(2)
                .Append(name)
                .Append(": ")
                .Append("cqrsClient.executeQuery.bind(cqrsClient, \"")
                .Append(statement.NamespaceName)
                .Append(".")
                .Append(statement.Name)
                .AppendLine("\"),");
        }

        private void VisitInterfaceStatement(InterfaceStatement statement, int level, string parentName)
        {
            if (!statement.IsStatic)
            {
                definitionsBuilder.AppendSpaces(level)
                    .Append("interface ")
                    .Append(statement.Name);

                if (statement.Parameters.Any())
                {
                    definitionsBuilder.Append("<");

                    for (int i = 0; i < statement.Parameters.Count; i++)
                    {
                        VisitTypeParameterStatement(statement.Parameters[i]);

                        if (i < statement.Parameters.Count - 1)
                        {
                            definitionsBuilder.Append(", ");
                        }
                    }

                    definitionsBuilder.Append(">");
                }

                if (statement.Extends.Any())
                {
                    definitionsBuilder.Append(" extends ");

                    for (int i = 0; i < statement.Extends.Count; i++)
                    {
                        VisitTypeStatement(statement.Extends[i]);

                        if (i < statement.Extends.Count - 1)
                        {
                            definitionsBuilder.Append(", ");
                        }
                    }
                }

                definitionsBuilder.AppendLine(" {");

                foreach (var field in statement.Fields)
                {
                    VisitFieldStatement(field, level + 1);
                }

                definitionsBuilder.AppendSpaces(level);
                definitionsBuilder.AppendLine("}");
                definitionsBuilder.AppendLine();
            }

            if (statement.Children.Any() || statement.Constants.Any())
            {
                definitionsBuilder.AppendSpaces(level);

                definitionsBuilder.Append("namespace ");
                definitionsBuilder.Append(statement.Name);
                definitionsBuilder.AppendLine(" {");

                constsBuilder.AppendSpaces(level);
                constsBuilder.Append(statement.Name);
                constsBuilder.AppendLine(": {");

                foreach (var child in statement.Children.Concat<IStatement>(statement.Constants))
                {
                    Visit(child, level + 1, parentName + "." + statement.Name);
                }

                constsBuilder.AppendSpaces(level);
                constsBuilder.AppendLine("},");

                definitionsBuilder.AppendSpaces(level);
                definitionsBuilder.AppendLine("}");
                definitionsBuilder.AppendLine();
            }
        }
    }
}


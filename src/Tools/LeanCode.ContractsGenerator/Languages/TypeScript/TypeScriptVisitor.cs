using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LeanCode.ContractsGenerator.Extensions;
using LeanCode.ContractsGenerator.Statements;

namespace LeanCode.ContractsGenerator.Languages.TypeScript
{
    internal class TypeScriptVisitor : ILanguageVisitor
    {
        private readonly StringBuilder definitionsBuilder;
        private readonly StringBuilder constsBuilder;
        private readonly StringBuilder clientBuilder;
        private readonly TypeScriptConfiguration configuration;
        private readonly string errorCodesName;

        public TypeScriptVisitor(TypeScriptConfiguration configuration, string errorCodesName)
        {
            this.configuration = configuration;
            this.errorCodesName = errorCodesName;

            definitionsBuilder = new StringBuilder();
            clientBuilder = new StringBuilder();
            constsBuilder = new StringBuilder();
        }

        public IEnumerable<LanguageFileOutput> Visit(ClientStatement statement)
        {
            var contractsBuilder = new StringBuilder(configuration.ContractsPreamble).AppendLine();

            Visit(statement, 0, null);

            contractsBuilder
                .Append(definitionsBuilder)
                .Append(constsBuilder)
                .Append(clientBuilder);

            yield return new LanguageFileOutput
            {
                Name = statement.Name + "Client.ts",
                Content = contractsBuilder.ToString(),
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
            clientBuilder.AppendLine("export default function (cqrsClient: CQRS) {");
            clientBuilder.AppendLine("    return {");

            definitionsBuilder.AppendSpaces(level);

            foreach (var child in statement.Children)
            {
                Visit(child, level, statement.Name);
            }

            clientBuilder.AppendLine("    };");
            clientBuilder.AppendLine("}");
        }

        private void VisitEnumStatement(EnumStatement statement, int level)
        {
            definitionsBuilder.AppendSpaces(level)
                .Append("export const enum ")
                .Append(statement.Name)
                .AppendLine(" {");

            foreach (var value in statement.Values)
            {
                VisitEnumValueStatement(value, level + 1);
            }

            definitionsBuilder.AppendSpaces(level)
                .AppendLine("}")
                .AppendLine();
        }

        private void VisitEnumValueStatement(EnumValueStatement statement, int level)
        {
            definitionsBuilder.AppendSpaces(level)
                .Append(statement.Name);

            if (!string.IsNullOrWhiteSpace(statement.Value))
            {
                definitionsBuilder.Append(" = ").Append(statement.Value);
            }

            definitionsBuilder.AppendLine(",");
        }

        private void VisitTypeStatement(TypeStatement statement, StringBuilder stringBuilder)
        {
            if (statement.IsDictionary)
            {
                stringBuilder.Append("{ [index: ");

                VisitTypeStatement(statement.TypeArguments.First(), stringBuilder);

                stringBuilder.Append("]: ");

                VisitTypeStatement(statement.TypeArguments.Last(), stringBuilder);

                stringBuilder.Append(" }");
            }
            else if (statement.IsArrayLike)
            {
                VisitTypeStatement(statement.TypeArguments.First(), stringBuilder);

                stringBuilder.Append("[]");
            }
            else if (statement.TypeArguments.Count > 0)
            {
                stringBuilder.Append(statement.Name);
                stringBuilder.Append("<");

                for (int i = 0; i < statement.TypeArguments.Count; i++)
                {
                    VisitTypeStatement(statement.TypeArguments[i], stringBuilder);

                    if (i < statement.TypeArguments.Count - 1)
                    {
                        stringBuilder.Append(", ");
                    }
                }

                stringBuilder.Append(">");
            }
            else if (configuration.TypeTranslations.TryGetValue(statement.Name.ToLower(), out string name))
            {
                stringBuilder.Append(name);
            }
            else
            {
                if (statement.ParentChain.Any())
                {
                    stringBuilder
                        .Append(string.Join("_", statement.ParentChain.Select(t => t.Name)))
                        .Append("_");
                }

                stringBuilder.Append(statement.Name);
            }
        }

        private void VisitConstStatement(ConstStatement statement, int level)
        {
            constsBuilder.AppendSpaces(level)
                .Append(statement.Name);

            if (!string.IsNullOrWhiteSpace(statement.Value))
            {
                constsBuilder.Append(": ").Append(statement.Value);
            }

            constsBuilder.AppendLine(",");
        }

        private void VisitTypeParameterStatement(TypeParameterStatement statement)
        {
            definitionsBuilder.Append(statement.Name);

            if (statement.Constraints.Any())
            {
                definitionsBuilder.Append(" extends ");

                for (int i = 0; i < statement.Constraints.Count; i++)
                {
                    VisitTypeStatement(statement.Constraints[i], definitionsBuilder);

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

            VisitTypeStatement(statement.Type, definitionsBuilder);

            if (statement.Type.IsNullable)
            {
                definitionsBuilder.Append(" | null");
            }

            definitionsBuilder.AppendLine(";");
        }

        private void VisitCommandStatement(CommandStatement statement, int level, string parentName)
        {
            VisitInterfaceStatement(statement, level, parentName);

            var name = char.ToLower(statement.Name[0]) + statement.Name.Substring(1);

            clientBuilder
                .AppendSpaces(2)
                .Append(name)
                .Append(": (dto: ")
                .Append(statement.Name)
                .Append(") => cqrsClient.executeCommand<");

            if (statement.Children.Any(c => c.Name == errorCodesName))
            {
                clientBuilder
                    .Append("typeof ")
                    .Append(statement.Name)
                    .Append($"[\"{errorCodesName}\"]");
            }
            else
            {
                clientBuilder.Append("{}");
            }

            clientBuilder
                .Append(">(\"")
                .Append(statement.Namespace)
                .Append(".")
                .Append(statement.Name)
                .Append("\", dto),")
                .AppendLine();
        }

        private void VisitQueryStatement(QueryStatement statement, int level, string parentName)
        {
            VisitInterfaceStatement(statement, level, parentName);

            var name = char.ToLower(statement.Name[0]) + statement.Name.Substring(1);

            clientBuilder.AppendSpaces(2)
                .Append(name)
                .Append(": (dto: ")
                .Append(statement.Name)
                .Append(") => cqrsClient.executeQuery<");

            VisitTypeStatement(statement.ResultType, clientBuilder);

            clientBuilder.Append(">(\"")
                .Append(statement.Namespace)
                .Append(".")
                .Append(statement.Name)
                .Append("\", dto),")
                .AppendLine();
        }

        private void VisitInterfaceStatement(InterfaceStatement statement, int level, string parentName)
        {
            if (!statement.IsStatic)
            {
                definitionsBuilder
                    .Append("export interface ");

                if (statement.ParentChain.Any())
                {
                    definitionsBuilder
                        .Append(string.Join("_", statement.ParentChain.Select(p => p.Name)))
                        .Append("_");
                }

                definitionsBuilder
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
                        VisitTypeStatement(statement.Extends[i], definitionsBuilder);

                        if (i < statement.Extends.Count - 1)
                        {
                            definitionsBuilder.Append(", ");
                        }
                    }
                }

                definitionsBuilder.AppendLine(" {");

                foreach (var field in statement.Fields)
                {
                    VisitFieldStatement(field, 1);
                }

                definitionsBuilder.AppendLine("}");
                definitionsBuilder.AppendLine();
            }

            if (statement.Children.Any() || statement.Constants.Any())
            {
                var hasConsts = HasNestedConstStatements(statement);

                if (hasConsts)
                {
                    if (level == 0)
                    {
                        constsBuilder
                            .Append("export const ")
                            .Append(statement.Name)
                            .AppendLine(" = {");
                    }
                    else
                    {
                        constsBuilder
                            .AppendSpaces(level)
                            .Append(statement.Name)
                            .AppendLine(": {");
                    }
                }

                foreach (var child in statement.Children.Concat<IStatement>(statement.Constants))
                {
                    Visit(child, level + 1, parentName + "." + statement.Name);
                }

                if (hasConsts)
                {
                    constsBuilder.AppendSpaces(level);
                    constsBuilder.Append("}");

                    if (level == 0)
                    {
                        constsBuilder
                            .AppendLine(";")
                            .AppendLine();
                    }
                    else
                    {
                        constsBuilder.AppendLine(",");
                    }
                }
            }
        }

        private bool HasNestedConstStatements(InterfaceStatement stmt)
        {
            return stmt.Constants.Any() ||
                stmt.Children.Any(HasNestedConstStatements);
        }
    }
}

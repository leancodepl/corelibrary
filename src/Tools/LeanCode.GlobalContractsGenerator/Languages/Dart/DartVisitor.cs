using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LeanCode.ContractsGenerator.Extensions;
using LeanCode.ContractsGenerator.Statements;

namespace LeanCode.ContractsGenerator.Languages.Dart
{
    class DartVisitor : ILanguageVisitor
    {
        private readonly StringBuilder definitionsBuilder = new StringBuilder();
        private readonly StringBuilder paramsBuilder = new StringBuilder();
        private readonly DartConfiguration configuration;
        private readonly HashSet<string> usedSymbols = new HashSet<string>();
        private string namespaceName;

        public DartVisitor(DartConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<LanguageFileOutput> Visit(ClientStatement statement)
        {
            definitionsBuilder.Append(configuration.ContractsPreamble).AppendLine();

            Visit(statement, 0, null);

            yield return new LanguageFileOutput
            {
                Name = statement.Name + ".dart",
                Content = definitionsBuilder.ToString()
            };

            yield return new LanguageFileOutput
            {
                Name = statement.Name + "_client.dart",
                Content = ""
            };
        }

        private void Visit(IStatement statement, int level, string parentName)
        {
            this.namespaceName = parentName;

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
            foreach (var child in statement.Children)
            {
                Visit(child, level, statement.Name);
            }
        }

        private void VisitEnumStatement(EnumStatement statement, int level)
        {
            var name = Mangle(namespaceName, statement.Name);

            if (usedSymbols.Contains(name))
            {
                name = Mangle(Mangle(namespaceName, statement.Namespace), name);
            }

            definitionsBuilder.AppendSpaces(level)
                .Append("class ")
                .Append(name)
                .AppendLine(" {");

            usedSymbols.Add(name);

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
                .Append("static const ")
                .Append(statement.Name);

            if (!string.IsNullOrWhiteSpace(statement.Value))
            {
                definitionsBuilder.Append(" = ").Append(statement.Value);
            }

            definitionsBuilder.AppendLine(";");
        }

        private void VisitTypeStatement(TypeStatement statement)
        {
            if (statement.IsDictionary)
            {
                definitionsBuilder.Append("Map<");

                VisitTypeStatement(statement.TypeArguments.First());

                definitionsBuilder.Append(",");

                VisitTypeStatement(statement.TypeArguments.Last());

                definitionsBuilder.Append(">");
            }
            else if (statement.IsArrayLike)
            {
                definitionsBuilder.Append("List<");

                VisitTypeStatement(statement.TypeArguments.First());

                definitionsBuilder.Append(">");
            }
            else if (statement.TypeArguments.Count > 0)
            {
                definitionsBuilder.Append(Mangle(namespaceName, statement.Name));
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
                definitionsBuilder.Append(Mangle(namespaceName, statement.Name));
            }
        }

        private void VisitConstStatement(ConstStatement statement, int level)
        {
            definitionsBuilder.AppendSpaces(level)
                .Append("const ")
                .Append(statement.Name);

            if (!string.IsNullOrWhiteSpace(statement.Value))
            {
                definitionsBuilder.Append(" = ").Append(statement.Value);
            }

            definitionsBuilder.AppendLine(";");
        }

        private void VisitTypeParameterStatement(TypeParameterStatement statement)
        {
            definitionsBuilder.Append(statement.Name);

            if (statement.Constraints.Any())
            {
                definitionsBuilder.Append(" implements ");

                for (int i = 0; i < statement.Constraints.Count; i++)
                {
                    VisitTypeStatement(statement.Constraints[i]);

                    if (i < statement.Constraints.Count - 1)
                    {
                        definitionsBuilder.Append(", ");
                    }
                }
            }
        }

        private void VisitFieldStatement(FieldStatement statement, int level)
        {
            definitionsBuilder.AppendSpaces(level);

            VisitTypeStatement(statement.Type);

            definitionsBuilder.Append(" ");
            definitionsBuilder.Append(statement.Name.Uncapitalize());
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
        }

        private void VisitInterfaceStatement(InterfaceStatement statement, int level, string parentName)
        {
            var name = Mangle(parentName, statement.Name);

            if (usedSymbols.Contains(name))
            {
                name = Mangle(Mangle(parentName, statement.Namespace), name);
            }

            if (!statement.IsStatic)
            {
                if (statement.Extends.Any(x => x.Name == "Enum"))
                {
                    VisitEnumStatement(new EnumStatement { Name = Mangle(parentName, statement.Name) }, level);
                    return;
                }

                definitionsBuilder.AppendSpaces(level)
                    .Append("class ")
                    .Append(name);

                usedSymbols.Add(name);

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
                    int i = 0;

                    if (statement.IsClass)
                    {
                        definitionsBuilder.Append(" extends ");
                        VisitTypeStatement(statement.Extends[0]);

                        if (statement.Extends.Count > 1)
                        {
                            definitionsBuilder.Append(" implements ");
                        }

                        i = 1;
                    }
                    else
                    {
                        definitionsBuilder.Append(" implements ");
                        i = 0;
                    }

                    for (; i < statement.Extends.Count; i++)
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
                foreach (var child in statement.Children)
                {
                    Visit(child, level, parentName + "." + statement.Name);
                }
            }
        }

        private string Mangle(string namespaceName, string identifier)
        {
            if (configuration.UnmangledTypes.Any(x => identifier == x))
                return identifier;

            return $"{namespaceName.Replace('.', '_')}_{identifier}";
        }
    }
}

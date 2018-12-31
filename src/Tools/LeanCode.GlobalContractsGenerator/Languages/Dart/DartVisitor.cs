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
                VisitEnumValueStatement(value, name, level + 1);
            }

            definitionsBuilder
                .AppendSpaces(level + 1)
                .AppendLine("final int value;")
                .AppendLine()
                .AppendSpaces(level + 1)
                .AppendLine("dynamic toJsonMap() => value;");

            definitionsBuilder.AppendSpaces(level)
                .AppendLine("}")
                .AppendLine();
        }

        private void VisitEnumValueStatement(EnumValueStatement statement, string parentName, int level)
        {
            definitionsBuilder.AppendSpaces(level)
                .Append("const ")
                .Append($"{parentName}.{statement.Name.Uncapitalize()}")
                .Append($"() : value = {statement.Value};")
                .AppendLine();
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
            VisitInterfaceStatement(statement, level, parentName, true);

            var name = statement.Name.Uncapitalize();

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
            VisitInterfaceStatement(statement, level, parentName, true, true);

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

        private void VisitInterfaceStatement(InterfaceStatement statement, int level, string parentName, bool includeFullName = false, bool includeResultFactory = false)
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

                if (includeFullName)
                {
                    definitionsBuilder
                        .AppendLine()
                        .AppendSpaces(level + 1)
                        .AppendLine("@override")
                        .AppendSpaces(level + 1)
                        .AppendLine($"String getFullName() => '{statement.Namespace}.{statement.Name}';");
                }

                if (includeResultFactory)
                {
                    GenerateResultFactory(statement, level);
                }

                GenerateToJsonMethod(statement, level);
                GenerateFromJsonMethod(name, statement.Fields, level);

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

        private void GenerateResultFactory(InterfaceStatement statement, int level)
        {
            var result = statement.Extends
                .Where(x => x.Name == "IRemoteQuery")
                .First()
                .TypeArguments.Last();

            definitionsBuilder
                .AppendLine()
                .AppendSpaces(level + 1)
                .AppendLine("@override")
                .AppendSpaces(level + 1);

            VisitTypeStatement(result);

            if (result.IsArrayLike)
            {
                definitionsBuilder
                    .AppendLine("resultFactory(decodedJson) {");

                definitionsBuilder
                    .AppendSpaces(level + 2)
                    .AppendLine("decodedJson")
                    .AppendSpaces(level + 3)
                    .Append(".map((x) => ");

                VisitTypeStatement(result.TypeArguments.First());

                definitionsBuilder
                    .AppendLine(".fromJson(x))")
                    .AppendSpaces(level + 3)
                    .AppendLine(".toList(growable: false);");
            }
            else
            {
                definitionsBuilder
                    .Append($"resultFactory(decodedJson) => ");

                VisitTypeStatement(result);
                definitionsBuilder
                    .AppendLine(".fromJson(decodedJson);");
            }

            definitionsBuilder
                .AppendSpaces(level + 1)
                .AppendLine("}");
        }

        private void GenerateToJsonMethod(InterfaceStatement statement, int level)
        {
            definitionsBuilder
                .AppendLine()
                .AppendSpaces(level + 1)
                .AppendLine("Map<String, dynamic> toJsonMap() {");

            definitionsBuilder
                .AppendSpaces(level + 2)
                .AppendLine("return <String, dynamic>{");

            foreach (var field in statement.Fields)
            {
                var serializedField = field.Name.Uncapitalize();

                if (!configuration.TypeTranslations.ContainsKey(field.Type.Name.ToLower()))
                {
                    serializedField += ".toJsonMap()";
                }

                definitionsBuilder
                    .AppendSpaces(level + 3)
                    .AppendLine($"'{field.Name.Capitalize()}': {serializedField},");
            }

            definitionsBuilder
                .AppendSpaces(level + 2)
                .AppendLine("};");

            definitionsBuilder
                .AppendSpaces(level + 1)
                .AppendLine("}");
        }

        private void GenerateFromJsonMethod(string name, List<FieldStatement> fields, int level)
        {
            definitionsBuilder
                .AppendLine()
                .AppendSpaces(level + 1)
                .Append($"static {name} fromJson(Map json) => {name}()");

            foreach (var field in fields)
            {
                var value = $"json['{field.Name.Capitalize()}']";

                if (field.Type.Name == "DateTime")
                {
                    value = $"DateTime.parse(normalizeDate({value}))";
                }
                else if (!configuration.TypeTranslations.ContainsKey(field.Type.Name.ToLower()))
                {
                    value = $"{field.Type.Name.Capitalize()}.fromJson({value})";
                }

                definitionsBuilder
                    .AppendLine()
                    .AppendSpaces(level + 2)
                    .Append($"..{field.Name.Uncapitalize()} = {value}");
            }

            definitionsBuilder
                .AppendLine(";");
        }

        private string Mangle(string namespaceName, string identifier)
        {
            if (configuration.UnmangledTypes.Any(x => identifier == x))
                return identifier;

            return $"{namespaceName.Replace('.', '_')}_{identifier}";
        }
    }
}

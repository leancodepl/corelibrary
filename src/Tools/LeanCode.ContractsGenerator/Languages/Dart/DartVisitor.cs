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
        private readonly DartConfiguration configuration;
        private Dictionary<string, string> mangledNames;

        public DartVisitor(DartConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<LanguageFileOutput> Visit(ClientStatement statement)
        {
            definitionsBuilder.Append(configuration.ContractsPreamble).AppendLine();

            GenerateTypeNames(statement);

            Visit(statement, 0, null);

            yield return new LanguageFileOutput
            {
                Name = statement.Name + ".dart",
                Content = definitionsBuilder.ToString()
            };
        }

        private void GenerateTypeNames(ClientStatement statement)
        {
            var symbols = new List<(string name, INamespacedStatement statement)>();

            foreach (var child in statement.Children)
            {
                if (child is InterfaceStatement || child is EnumStatement)
                {
                    symbols.Add((child.Name, child as INamespacedStatement));

                    if (child is InterfaceStatement iStmt)
                    {
                        symbols.AddRange(iStmt.Children.Select(x => (x.Name, x as INamespacedStatement)));
                    }
                }
            }

            mangledNames = symbols
                .GroupBy(x => x.name)
                .Select(MangleGroup)
                .SelectMany(x => x)
                .ToDictionary(x => Mangle(x.statement.Namespace, x.statement.Name), x => x.name);
        }

        private void Visit(IStatement statement, int level, string parentName)
        {
            switch (statement)
            {
                case ClientStatement stmt: VisitClientStatement(stmt, level); break;
                case EnumStatement stmt: VisitEnumStatement(stmt, level); break;

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
            var name = mangledNames[Mangle(statement.Namespace, statement.Name)];

            definitionsBuilder.AppendSpaces(level)
                .Append("class ")
                .Append(name)
                .AppendLine(" {");

            foreach (var value in statement.Values)
            {
                VisitEnumValueStatement(value, name, level + 1);
            }

            definitionsBuilder
                .AppendSpaces(level + 1)
                .AppendLine("final int value;")
                .AppendLine()
                .AppendSpaces(level + 1)
                .AppendLine($"{name}._(this.value);")
                .AppendLine()
                .AppendSpaces(level + 1)
                .AppendLine("dynamic toJsonMap() => value;")
                .AppendSpaces(level + 1)
                .AppendLine($"static {name} fromJson(dynamic json) => {name}._(json);");

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

                definitionsBuilder.Append(", ");

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
                var name = statement.Name;

                if (!configuration.UnmangledTypes.Contains(statement.Name))
                    name = mangledNames[Mangle(statement.Namespace, statement.Name)];

                definitionsBuilder.Append(name);
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
            else if (configuration.TypeTranslations.TryGetValue(statement.Name.ToLower(), out string newName))
            {
                definitionsBuilder.Append(newName);
            }
            else
            {
                var name = statement.Name;

                if (!configuration.UnmangledTypes.Contains(statement.Name))
                    if (!mangledNames.TryGetValue(Mangle(statement.Namespace, statement.Name), out name))
                        name = statement.Name;

                definitionsBuilder.Append(name);
            }
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
        }

        private void VisitQueryStatement(QueryStatement statement, int level, string parentName)
        {
            VisitInterfaceStatement(statement, level, parentName, true, true);
        }

        private void VisitInterfaceStatement(InterfaceStatement statement, int level, string parentName, bool includeFullName = false, bool includeResultFactory = false)
        {
            var name = mangledNames[Mangle(statement.Namespace, statement.Name)];

            if (statement.Extends.Any(x => x.Name == "Enum"))
            {
                VisitEnumStatement(new EnumStatement { Name = name }, level);
                return;
            }

            definitionsBuilder.AppendSpaces(level)
                .Append("class ")
                .Append(name);

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

            foreach (var constant in statement.Constants)
            {
                definitionsBuilder
                    .AppendSpaces(level + 1)
                    .AppendLine($"static const int {constant.Name} = {constant.Value};");
            }

            definitionsBuilder.AppendLine();

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

            if (statement.Children.Any())
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
                    .AppendLine(" resultFactory(dynamic decodedJson) {");

                definitionsBuilder
                    .AppendSpaces(level + 2)
                    .AppendLine("return decodedJson")
                    .AppendSpaces(level + 3)
                    .Append(".map((dynamic x) => ");

                VisitTypeStatement(result.TypeArguments.First());

                definitionsBuilder
                    .AppendLine(".fromJson(x))")
                    .AppendSpaces(level + 3)
                    .AppendLine(".toList(growable: false)")
                    .AppendSpaces(level + 3)
                    .Append(".cast<");

                VisitTypeStatement(result.TypeArguments.First());

                definitionsBuilder
                    .AppendLine(">();");

                definitionsBuilder
                    .AppendSpaces(level + 1)
                    .AppendLine("}");
            }
            else
            {
                definitionsBuilder
                    .Append($" resultFactory(dynamic decodedJson) => ");

                VisitTypeStatement(result);
                definitionsBuilder
                    .AppendLine(".fromJson(decodedJson);");
            }
        }

        private void GenerateToJsonMethod(InterfaceStatement statement, int level)
        {
            definitionsBuilder
                .AppendSpaces(level + 1)
                .AppendLine("Map<String, dynamic> toJsonMap() {");

            definitionsBuilder
                .AppendSpaces(level + 2)
                .AppendLine("return <String, dynamic>{");

            foreach (var field in statement.Fields)
            {
                var serializedField = field.Name.Uncapitalize();

                if (field.Type.IsArrayLike)
                {
                    serializedField = $"json.encode({serializedField})";
                }
                else if (!configuration.TypeTranslations.ContainsKey(field.Type.Name.ToLower()))
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
                .Append($"static {name} fromJson(Map map) => {name}()");

            foreach (var field in fields)
            {
                var value = $"map['{field.Name.Capitalize()}']";

                if (field.Type.Name == "DateTime")
                {
                    value = $"DateTime.parse(normalizeDate({value}))";
                }

                if (field.Type.IsArrayLike || field.Type.IsDictionary)
                {
                    definitionsBuilder
                        .AppendLine()
                        .AppendSpaces(level + 2)
                        .AppendLine($"..{field.Name.Uncapitalize()} = {value}")
                        .AppendSpaces(level + 3)
                        .Append(".map((dynamic x) => ");

                    VisitTypeStatement(field.Type.TypeArguments.First());

                    definitionsBuilder
                        .AppendLine(".fromJson(x))")
                        .AppendSpaces(level + 3)
                        .AppendLine(".toList(growable: false)")
                        .AppendSpaces(level + 3)
                        .Append(".cast<");

                    VisitTypeStatement(field.Type.TypeArguments.First());

                    definitionsBuilder
                        .Append(">()");

                    continue;
                }

                if (!configuration.TypeTranslations.ContainsKey(field.Type.Name.ToLower()))
                {
                    definitionsBuilder
                        .AppendLine()
                        .AppendSpaces(level + 2)
                        .Append($"..{field.Name.Uncapitalize()} = ");

                    VisitTypeStatement(field.Type);

                    definitionsBuilder.Append($".fromJson({value})");
                }
                else
                {
                    definitionsBuilder
                        .AppendLine()
                        .AppendSpaces(level + 2)
                        .Append($"..{field.Name.Uncapitalize()} = {value}");
                }
            }

            definitionsBuilder
                .AppendLine(";");
        }

        private string Mangle(string namespaceName, string identifier)
        {
            if (configuration.UnmangledTypes.Any(x => identifier == x))
                return identifier;

            if (string.IsNullOrEmpty(namespaceName))
                return identifier;

            return $"{namespaceName}.{identifier}".Replace('.', '_');
        }

        private IList<(string name, INamespacedStatement statement)> MangleGroup(IGrouping<string, (string name, INamespacedStatement statement)> group)
        {
            var mangle = group.Count() > 1;

            return group
                .Select(x => (mangle ? Mangle(x.statement.Namespace, x.name) : x.name, x.statement))
                .ToList();
        }
    }
}

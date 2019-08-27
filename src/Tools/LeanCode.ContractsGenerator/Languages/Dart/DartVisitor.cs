using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LeanCode.ContractsGenerator.Extensions;
using LeanCode.ContractsGenerator.Statements;

namespace LeanCode.ContractsGenerator.Languages.Dart
{
    internal class DartVisitor : ILanguageVisitor
    {
        private readonly StringBuilder definitionsBuilder = new StringBuilder();
        private readonly DartConfiguration configuration;
        private readonly List<string> primitiveTypes;
        private Dictionary<string, (string name, INamespacedStatement statement)> mangledStatements;

        public DartVisitor(DartConfiguration configuration)
        {
            this.configuration = configuration;
            primitiveTypes = configuration.TypeTranslations.Keys
                .Select(name => name.ToLowerInvariant())
                .ToList();
        }

        public IEnumerable<LanguageFileOutput> Visit(ClientStatement statement)
        {
            definitionsBuilder.Append(configuration.ContractsPreamble).AppendLine();

            GenerateDartAnnotations();

            GenerateTypeNames(statement);

            Visit(statement, 0, null);

            yield return new LanguageFileOutput
            {
                Name = statement.Name + ".dart",
                Content = definitionsBuilder.ToString(),
            };
        }

        private void GenerateDartAnnotations()
        {
            definitionsBuilder.AppendLine("class _Nullable { const _Nullable(); } const nullable = _Nullable();");
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

            mangledStatements = symbols
                .GroupBy(x => x.name)
                .Select(MangleGroup)
                .SelectMany(x => x)
                .ToDictionary(x => Mangle(x.statement.Namespace, x.statement.Name), x => (x.name, x.statement));
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
            var name = mangledStatements[Mangle(statement.Namespace, statement.Name)].name;

            definitionsBuilder.AppendSpaces(level)
                .Append("class ")
                .Append(name)
                .AppendLine(" {")
                .AppendSpaces(level + 1)
                .AppendLine($"const {name}(this.value);");

            foreach (var value in statement.Values)
            {
                VisitEnumValueStatement(value, name, level + 1);
            }

            definitionsBuilder
                .AppendSpaces(level + 1)
                .AppendLine("final int value;")
                .AppendLine()
                .AppendSpaces(level + 1)
                .AppendLine("dynamic toJsonMap() => value;")
                .AppendLine()
                .AppendSpaces(level + 1)
                .AppendLine($"static {name} fromJson(dynamic json) => {name}(json);")
                .AppendLine();

            AppendEnumEqualityOperator(name, level + 1);

            AppendEnumHashCode(name, level + 1);

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

        private void AppendEnumEqualityOperator(string parentName, int level)
        {
            definitionsBuilder
                .AppendSpaces(level)
                .AppendLine("@override")
                .AppendSpaces(level)
                .AppendLine("bool operator ==(Object other) =>")
                .AppendSpaces(level + 1)
                .AppendLine($"other is {parentName} && value == other.value;")
                .AppendLine();
        }

        private void AppendEnumHashCode(string parentName, int level)
        {
            definitionsBuilder
                .AppendSpaces(level)
                .AppendLine("@override")
                .AppendSpaces(level)
                .AppendLine("int get hashCode => value.hashCode;");
        }

        private void VisitTypeStatement(TypeStatement statement)
        {
            if (statement.IsNullable)
            {
                definitionsBuilder.AppendLine("@nullable");
            }

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
                {
                    name = mangledStatements[Mangle(statement.Namespace, statement.Name)].name;
                }

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
            else if (configuration.TypeTranslations.TryGetValue(statement.Name.ToLowerInvariant(), out string newName))
            {
                definitionsBuilder.Append(newName);
            }
            else
            {
                var name = statement.Name;

                if (!configuration.UnmangledTypes.Contains(statement.Name))
                {
                    if (mangledStatements.TryGetValue(Mangle(statement.Namespace, statement.Name), out var type))
                    {
                        name = type.name;
                    }
                }

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
            var name = mangledStatements[Mangle(statement.Namespace, statement.Name)].name;

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

            var mapJsonIncludeSuper = false;

            if (statement.Extends.Any())
            {
                int i = 0;

                if (statement.IsClass)
                {
                    mapJsonIncludeSuper = true;
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
                    .AppendLine($"String getFullName() => '{statement.Namespace}.{statement.Name}';")
                    .AppendLine();
            }

            if (includeResultFactory)
            {
                GenerateResultFactory(statement, level);
            }

            var includeOverrideAnnotation = includeFullName || statement.Extends.Any();
            GenerateToJsonMethod(statement, level, includeOverrideAnnotation, mapJsonIncludeSuper);
            GenerateFromJsonMethod(name, statement, level);

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
                    .Append("?.map((dynamic x) => ");

                VisitTypeStatement(result.TypeArguments.First());

                definitionsBuilder
                    .AppendLine(".fromJson(x))")
                    .AppendSpaces(level + 3)
                    .AppendLine("?.toList(growable: false)")
                    .AppendSpaces(level + 3)
                    .Append("?.cast<");

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

                if (!primitiveTypes.Contains(result.Name.ToLowerInvariant()))
                {
                    VisitTypeStatement(result);
                    definitionsBuilder
                        .AppendLine(".fromJson(decodedJson);");
                }
                else
                {
                    definitionsBuilder.Append("decodedJson as ");
                    VisitTypeStatement(result);
                    definitionsBuilder.AppendLine(";");
                }
            }
        }

        private void GenerateToJsonMethod(InterfaceStatement statement, int level, bool includeOverrideAnnotation, bool includeSuper)
        {
            var annotation = includeOverrideAnnotation ? "@override" : "@virtual";

            definitionsBuilder
                .AppendLine()
                .AppendSpaces(level + 1)
                .AppendLine(annotation)
                .AppendSpaces(level + 1)
                .AppendLine("Map<String, dynamic> toJsonMap() {");

            definitionsBuilder
                .AppendSpaces(level + 2)
                .AppendLine("final map = <String, dynamic>{");

            foreach (var field in statement.Fields)
            {
                var serializedField = field.Name.Uncapitalize();

                definitionsBuilder
                    .AppendSpaces(level + 3)
                    .Append($"'{field.Name.Capitalize()}': {serializedField}");

                GenerateTypeMapingForToJsonMethod(field.Type, 0);

                definitionsBuilder.AppendLine(",");
            }

            definitionsBuilder
                .AppendSpaces(level + 2)
                .AppendLine("};");

            if (includeSuper)
            {
                definitionsBuilder
                    .AppendSpaces(level + 2)
                    .AppendLine("map.addAll(super.toJsonMap());");
            }

            definitionsBuilder
                .AppendSpaces(level + 2)
                .AppendLine("return map;")
                .AppendSpaces(level + 1)
                .AppendLine("}");
        }

        private void GenerateTypeMapingForToJsonMethod(TypeStatement statement, int depth)
        {
            if (statement.IsArrayLike)
            {
                if (statement.TypeArguments.FirstOrDefault() is TypeStatement argumentType)
                {
                    if (!configuration.TypeTranslations.ContainsKey(argumentType.Name.ToLowerInvariant()))
                    {
                        definitionsBuilder.Append($".map((x{depth}) => x{depth}");
                        GenerateTypeMapingForToJsonMethod(argumentType, depth + 1);
                        definitionsBuilder.Append(").toList()");
                    }
                }
            }
            else if (statement.Name == "DateTime" && !statement.IsNullable)
            {
                definitionsBuilder.Append(".toIso8601String()");
            }
            else if (statement.Name == "DateTime" && statement.IsNullable)
            {
                definitionsBuilder.Append("?.toIso8601String()");
            }
            else if (!configuration.TypeTranslations.ContainsKey(statement.Name.ToLowerInvariant()))
            {
                definitionsBuilder.Append(".toJsonMap()");
            }
        }

        private void GenerateFromJsonMethod(string name, InterfaceStatement statement, int level)
        {
            definitionsBuilder
                .AppendLine()
                .AppendSpaces(level + 1)
                .Append($"static {name} fromJson(Map map) => {name}()");

            GenerateFromJsonAssignments(statement.Fields, level + 1);

            if (statement.Extends.FirstOrDefault() is TypeStatement baseClass && statement.IsClass)
            {
                var type = mangledStatements[Mangle(baseClass.Namespace, baseClass.Name)];

                if (type.statement is InterfaceStatement baseStatement)
                {
                    GenerateFromJsonAssignments(baseStatement.Fields, level + 1);
                }
            }

            definitionsBuilder.AppendLine(";");
        }

        private void GenerateFromJsonAssignments(List<FieldStatement> fields, int level)
        {
            foreach (var field in fields)
            {
                var map = $"map['{field.Name.Capitalize()}']";
                var value = map;

                if (field.Type.IsArrayLike || field.Type.IsDictionary)
                {
                    definitionsBuilder
                        .AppendLine()
                        .AppendSpaces(level + 2)
                        .AppendLine($"..{field.Name.Uncapitalize()} = {value}")
                        .AppendSpaces(level + 3);

                    var argType = field.Type.TypeArguments.First();

                    if (!configuration.TypeTranslations.ContainsKey(argType.Name.ToLowerInvariant()))
                    {
                        definitionsBuilder
                           .Append("?.map((dynamic x) => ");

                        VisitTypeStatement(argType);

                        definitionsBuilder
                            .AppendLine(".fromJson(x))");
                    }
                    else
                    {
                        definitionsBuilder.Append("?.map((dynamic x) => x)");
                    }

                    definitionsBuilder
                        .AppendSpaces(level + 3)
                        .AppendLine("?.toList(growable: false)")
                        .AppendSpaces(level + 3)
                        .Append("?.cast<");

                    VisitTypeStatement(field.Type.TypeArguments.First());

                    definitionsBuilder
                        .Append(">()");

                    continue;
                }

                if (!configuration.TypeTranslations.ContainsKey(field.Type.Name.ToLowerInvariant()))
                {
                    definitionsBuilder
                        .AppendLine()
                        .AppendSpaces(level + 2)
                        .Append($"..{field.Name.Uncapitalize()} = {map} != null ? ");

                    VisitTypeStatement(field.Type);

                    definitionsBuilder.Append($".fromJson({value}) : null");
                }
                else
                {
                    definitionsBuilder
                        .AppendLine()
                        .AppendSpaces(level + 2);

                    if (field.Type.Name == "DateTime")
                    {
                        value = $"DateTime.parse(normalizeDate({value}))";
                        definitionsBuilder.Append($"..{field.Name.Uncapitalize()} = {map} != null ? {value} : null");
                    }
                    else if (field.Type.Name == "Double")
                    {
                        definitionsBuilder.AppendLine($"..{field.Name.Uncapitalize()} = {map} is String ?")
                            .AppendLine($"double.parse({map}) : {map}");
                    }
                    else
                    {
                        definitionsBuilder
                            .Append($"..{field.Name.Uncapitalize()} = {value}");
                    }
                }
            }
        }

        private string Mangle(string namespaceName, string identifier)
        {
            if (configuration.UnmangledTypes.Any(x => identifier == x))
            {
                return identifier;
            }

            if (string.IsNullOrEmpty(namespaceName))
            {
                return identifier;
            }

            return $"{namespaceName}.{identifier}".Replace('.', '_');
        }

        private IList<(string name, INamespacedStatement statement)> MangleGroup(IGrouping<string, (string name, INamespacedStatement statement)> group)
        {
            var mangle = group.Count() > 1;

            if (!mangle)
            {
                return group.Select(x => (x.name, x.statement)).ToList();
            }

            var limit = group.Select(s => s.statement.Namespace.Split('.').Count()).Max();

            int depth = 1;

            while (depth <= limit)
            {
                var groups = group.Select(g => (name: MakeName(g.statement.Namespace, g.name, depth), g.statement))
                            .GroupBy(g => g.name);

                if (groups.Any(g => g.Count() > 1))
                {
                    ++depth;
                }
                else
                {
                    return groups.Select(x => (x.First().name, x.First().statement)).ToList();
                }
            }

            return group
                .Select(x => (mangle ? Mangle(x.statement.Namespace, x.name) : x.name, x.statement))
                .ToList();
        }

        private string MakeName(string namespaceName, string name, int depth)
        {
            var split = namespaceName.Split('.').Reverse().Take(depth).Append(name);
            return string.Join(string.Empty, split);
        }
    }
}

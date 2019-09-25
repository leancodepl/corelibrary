using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using LeanCode.ContractsGenerator.Extensions;
using LeanCode.ContractsGenerator.Statements;

namespace LeanCode.ContractsGenerator.Languages.Dart
{
    internal class DartVisitor : ILanguageVisitor
    {
        private static readonly HashSet<string> BuiltinTypes = new HashSet<string>
        {
            "int",
            "double",
            "float",
            "single",
            "int32",
            "uint32",
            "byte",
            "sbyte",
            "int64",
            "short",
            "long",
            "decimal",
            "bool",
            "boolean",
            "guid",
            "string",
        };
        private readonly StringBuilder definitionsBuilder = new StringBuilder();
        private readonly DartConfiguration configuration;
        private Dictionary<string, (string name, INamespacedStatement statement)> mangledStatements;

        public DartVisitor(DartConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<LanguageFileOutput> Visit(ClientStatement statement)
        {
            AddContractsPreamble(statement);

            GenerateHelpers();

            GenerateTypeNames(statement);

            Visit(statement, 0, null);

            yield return new LanguageFileOutput
            {
                Name = statement.Name + ".dart",
                Content = definitionsBuilder.ToString(),
            };
        }

        private void AddContractsPreamble(ClientStatement statement)
        {
            if (configuration.ContractsPreambleLines == DartConfiguration.DefaultPreambleLines)
            {
                var defaultPreamble = configuration.ContractsPreamble.Replace("{0}", statement.Name);
                definitionsBuilder.AppendLine(defaultPreamble);
            }
            else
            {
                definitionsBuilder.AppendLine(configuration.ContractsPreamble);
            }
        }

        private void GenerateHelpers()
        {
            definitionsBuilder
                .AppendLine("List<T> _listFromJson<T>(")
                .AppendLine("dynamic decodedJson, T itemFromJson(Map<String, dynamic> map)) {")
                .AppendLine("return decodedJson?.map((dynamic e) => itemFromJson(e))?.toList()?.cast<T>(); }");

            definitionsBuilder
                .AppendLine("DateTime _dateTimeFromJson(String value) {")
                .AppendLine("return DateTime.parse(value.substring(0, 19) + ' Z'); }");

            definitionsBuilder
                .AppendLine("DateTime _nullableDateTimeFromJson(String value) {")
                .AppendLine("return value == null ? null : _dateTimeFromJson(value); }");

            definitionsBuilder
                .AppendLine("double _doubleFromJson(dynamic value) {")
                .AppendLine("return value is String ? double.parse(value) : value; }");

            definitionsBuilder
                .AppendLine("double _nullableDoubleFromJson(dynamic value) {")
                .AppendLine("return value == null ? null : _doubleFromJson(value); }");
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
                .Append("enum ")
                .Append(name)
                .AppendLine(" {")
                .AppendSpaces(level + 1);

            foreach (var value in statement.Values)
            {
                VisitEnumValueStatement(value, level + 1);
            }

            definitionsBuilder.AppendSpaces(level)
                .AppendLine("}")
                .AppendLine();

            return;
        }

        private void VisitEnumValueStatement(EnumValueStatement statement, int level)
        {
            definitionsBuilder.AppendSpaces(level)
                .Append("@JsonValue(");

            if (int.TryParse(statement.Value, out int value))
            {
                definitionsBuilder.Append(value);
            }
            else
            {
                definitionsBuilder.Append($"\"{statement.Value}\"");
            }

            definitionsBuilder.AppendLine(")");

            definitionsBuilder.AppendSpaces(level)
                .Append(TranslateIdentifier(statement.Name))
                .AppendLine(",");
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
            GenerateJsonKey(statement, level);

            definitionsBuilder.AppendSpaces(level);

            VisitTypeStatement(statement.Type);

            definitionsBuilder.Append(" ")
                .Append(TranslateIdentifier(statement.Name))
                .AppendLine(";");
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

            definitionsBuilder.AppendLine()
                .AppendSpaces(level)
                .AppendLine("@JsonSerializable()");

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

            if (statement.IsClass && statement.BaseClass != null)
            {
                mapJsonIncludeSuper = true;
                definitionsBuilder.Append(" extends ");
                VisitTypeStatement(statement.BaseClass);
            }

            var mappedInterfaces = statement.Extends
                .Where(e => e.Name.StartsWith("IRemoteQuery") || e.Name.StartsWith("IRemoteCommand"))
                .ToList();

            if (mappedInterfaces.Any())
            {
                definitionsBuilder.Append(" implements ");

                for (var i = 0; i < mappedInterfaces.Count; i++)
                {
                    VisitTypeStatement(mappedInterfaces[i]);

                    if (i < mappedInterfaces.Count - 1)
                    {
                        definitionsBuilder.Append(", ");
                    }
                }
            }

            definitionsBuilder.AppendLine(" {");

            GenerateDefaultConstructor(name, level);

            foreach (var constant in statement.Constants)
            {
                definitionsBuilder
                    .AppendSpaces(level + 1)
                    .AppendLine($"static const int {constant.Name} = {constant.Value};");
            }

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
            GenerateToJsonMethod(statement, name, level, includeOverrideAnnotation, mapJsonIncludeSuper);
            GenerateFromJsonConstructor(name, statement, level);

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

            definitionsBuilder.Append(" resultFactory(dynamic decodedJson) => ");

            if (result.IsArrayLike)
            {
                result = result.TypeArguments.First();

                if (BuiltinTypes.Contains(result.Name.ToLowerInvariant()))
                {
                    definitionsBuilder.AppendLine("decodedJson.cast<");

                    VisitTypeStatement(result);

                    definitionsBuilder.AppendLine(">();");
                }
                else
                {
                    var name = mangledStatements[Mangle(result.Namespace, result.Name)].name;

                    definitionsBuilder.AppendLine($"_listFromJson(decodedJson, _${name}FromJson);");
                }
            }
            else
            {
                if (!BuiltinTypes.Contains(result.Name.ToLowerInvariant()))
                {
                    var name = mangledStatements[Mangle(result.Namespace, result.Name)].name;

                    definitionsBuilder.AppendLine($"_${name}FromJson(decodedJson);");
                }
                else
                {
                    definitionsBuilder.Append("decodedJson as ");
                    VisitTypeStatement(result);
                    definitionsBuilder.AppendLine(";");
                }
            }
        }

        private void GenerateJsonKey(FieldStatement statement, int level)
        {
            var type = statement.Type;

            definitionsBuilder
                .AppendSpaces(level)
                .Append("@JsonKey(")
                .Append($"name: '{statement.Name}'");

            if (type.IsNullable)
            {
                definitionsBuilder.Append(", nullable: true");
            }

            if (type.Name == "DateTime")
            {
                var fromJsonMethod = type.IsNullable ? "_nullableDateTimeFromJson" : "_dateTimeFromJson";
                definitionsBuilder.Append($", fromJson: {fromJsonMethod}");
            }
            else if (type.Name == "Double")
            {
                var fromJsonMethod = type.IsNullable ? "_nullableDoubleFromJson" : "_doubleFromJson";
                definitionsBuilder.Append($", fromJson: {fromJsonMethod}");
            }

            definitionsBuilder.AppendLine(")");
        }

        private void GenerateDefaultConstructor(string name, int level)
        {
            definitionsBuilder
                .AppendLine()
                .AppendSpaces(level + 1)
                .AppendLine($"{name}();");
        }

        private void GenerateToJsonMethod(InterfaceStatement statement, string fullName, int level, bool includeOverrideAnnotation, bool includeSuper)
        {
            var annotation = includeOverrideAnnotation ? "@override" : "@virtual";

            definitionsBuilder
                .AppendLine()
                .AppendSpaces(level + 1)
                .AppendLine(annotation)
                .AppendSpaces(level + 1)
                .Append("Map<String, dynamic> toJsonMap() => ")
                .AppendLine($"_${fullName}ToJson(this);");
        }

        private void GenerateFromJsonConstructor(string name, InterfaceStatement statement, int level)
        {
            definitionsBuilder.AppendLine()
                .AppendSpaces(level + 1)
                .Append($"factory {name}.fromJson(Map<String, dynamic> json) => ")
                .AppendLine($"_${name}FromJson(json);");
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

        private string TranslateIdentifier(string identifier)
        {
            var translated = identifier.Uncapitalize();

            if (translated == "new")
            {
                translated += '_';
            }

            return translated;
        }
    }
}

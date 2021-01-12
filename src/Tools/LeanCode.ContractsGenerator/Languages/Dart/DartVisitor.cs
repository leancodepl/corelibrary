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
        private readonly StringBuilder definitionsBuilder = new StringBuilder();
        private readonly DartConfiguration configuration;
        private Dictionary<string, (string name, INamespacedStatement statement)> mangledStatements = null!;

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

            yield return new LanguageFileOutput(
                statement.Name + ".dart",
                definitionsBuilder.ToString());
        }

        private void AddContractsPreamble(ClientStatement statement)
        {
            var defaultPreamble = DartConfiguration.DefaultContractsPreamble;
            var preamble = configuration.ContractsPreamble;

            if (preamble == defaultPreamble)
            {
                preamble = string.Format(defaultPreamble, statement.Name);
            }

            definitionsBuilder.AppendLine(preamble);
        }

        private void GenerateHelpers()
        {
            definitionsBuilder
                .AppendLine("List<T> _listFromJson<T>(Iterable<dynamic> decodedJson, T itemFromJson(Map<String, dynamic> map)) {")
                .AppendLine("    return decodedJson")
                .AppendLine("        ?.map((dynamic e) => itemFromJson(e as Map<String,dynamic>))")
                .AppendLine("        ?.toList()")
                .AppendLine("        ?.cast<T>();")
                .AppendLine("}");

            definitionsBuilder
                .AppendLine("DateTime _dateTimeFromJson(String value) {")
                .AppendLine("    return DateTime.parse('${value.substring(0, 19)} Z');")
                .AppendLine("}");

            definitionsBuilder
                .AppendLine("DateTime _nullableDateTimeFromJson(String value) {")
                .AppendLine("    return value == null ? null : _dateTimeFromJson(value);")
                .AppendLine("}");

            definitionsBuilder
                .AppendLine("double _doubleFromJson(dynamic value) {")
                .AppendLine("    if (value is double) { return value; }")
                .AppendLine("    else if (value is String) { return double.parse(value); }")
                .AppendLine("    else if (value is int) { return value.toDouble(); }")
                .AppendLine("    else { throw Exception('Invalid argument type ${value.runtimeType}'); }")
                .AppendLine("}");

            definitionsBuilder
                .AppendLine("double _nullableDoubleFromJson(dynamic value) {")
                .AppendLine("    return value == null ? null : _doubleFromJson(value);")
                .AppendLine("}");
        }

        private void GenerateTypeNames(ClientStatement statement)
        {
            var symbols = new List<(string name, INamespacedStatement statement)>();

            foreach (var child in statement.Children)
            {
                if (child is InterfaceStatement iStmt)
                {
                    symbols.Add((child.Name, iStmt));
                    symbols.AddRange(iStmt.Children.Select(x => (x.Name, x as INamespacedStatement)));
                }
                else if (child is EnumStatement enumStmt)
                {
                    symbols.Add((child.Name, enumStmt));
                }
            }

            mangledStatements = symbols
                .GroupBy(x => x.name)
                .Select(MangleGroup)
                .SelectMany(x => x)
                .ToDictionary(x => Mangle(x.statement.Namespace, x.statement.Name), x => (x.name, x.statement));
        }

        private void Visit(IStatement statement, int level, string? parentName)
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
                .AppendSpaces(level);

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
                .Append("@JsonValue(");

            if (int.TryParse(statement.Value, out var value))
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

                definitionsBuilder.Append('>');
            }
            else if (statement.IsArrayLike)
            {
                definitionsBuilder.Append("List<");

                VisitTypeStatement(statement.TypeArguments.First());

                definitionsBuilder.Append('>');
            }
            else if (statement.TypeArguments.Count > 0)
            {
                var name = statement.Name;

                if (!configuration.UnmangledTypes.Contains(statement.Name))
                {
                    name = mangledStatements[Mangle(statement.Namespace, statement.Name)].name;
                }

                definitionsBuilder.Append(name);
                definitionsBuilder.Append('<');

                for (var i = 0; i < statement.TypeArguments.Count; i++)
                {
                    VisitTypeStatement(statement.TypeArguments[i]);

                    if (i < statement.TypeArguments.Count - 1)
                    {
                        definitionsBuilder.Append(", ");
                    }
                }

                definitionsBuilder.Append('>');
            }
            else if (configuration.TypeTranslations.TryGetValue(statement.Name.ToLowerInvariant(), out var newName))
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

                for (var i = 0; i < statement.Constraints.Count; i++)
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

            definitionsBuilder.Append(' ')
                .Append(TranslateIdentifier(statement.Name))
                .AppendLine(";");
        }

        private void VisitCommandStatement(CommandStatement statement, int level, string? parentName)
        {
            VisitInterfaceStatement(statement, level, parentName, true);
        }

        private void VisitQueryStatement(QueryStatement statement, int level, string? parentName)
        {
            VisitInterfaceStatement(statement, level, parentName, true, true);
        }

        private void VisitInterfaceStatement(InterfaceStatement statement, int level, string? parentName, bool includeFullName = false, bool includeResultFactory = false)
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
                definitionsBuilder.Append('<');

                for (var i = 0; i < statement.Parameters.Count; i++)
                {
                    VisitTypeParameterStatement(statement.Parameters[i]);

                    if (i < statement.Parameters.Count - 1)
                    {
                        definitionsBuilder.Append(", ");
                    }
                }

                definitionsBuilder.Append('>');
            }

            if (statement.IsClass && statement.BaseClass != null)
            {
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
            GenerateFromJsonConstructor(name, level);
            GenerateConstants(statement.Constants, level);
            GenerateFields(statement.Fields, level);

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

            var includeOverrideAnnotation = includeFullName || statement.Extends.Any();
            GenerateToJsonMethod(name, level, includeOverrideAnnotation);

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

                var typeKey = Mangle(result.Namespace, result.Name);

                if (mangledStatements.ContainsKey(typeKey))
                {
                    var name = mangledStatements[typeKey].name;

                    definitionsBuilder.AppendLine($"_listFromJson(decodedJson as Iterable<dynamic>, _${name}FromJson);");
                }
                else
                {
                    definitionsBuilder.AppendLine("decodedJson.cast<");

                    VisitTypeStatement(result);

                    definitionsBuilder.AppendLine(">();");
                }
            }
            else
            {
                var typeKey = Mangle(result.Namespace, result.Name);

                if (mangledStatements.ContainsKey(typeKey))
                {
                    var name = mangledStatements[typeKey].name;
                    definitionsBuilder.AppendLine($"_${name}FromJson(decodedJson as Map<String, dynamic>);");
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

            if (IsA<DateTime>(type))
            {
                var fromJsonMethod = type.IsNullable ? "_nullableDateTimeFromJson" : "_dateTimeFromJson";
                definitionsBuilder.Append($", fromJson: {fromJsonMethod}");
            }
            else if (IsA<double>(type) || IsA<decimal>(type) || IsA<float>(type))
            {
                var fromJsonMethod = type.IsNullable ? "_nullableDoubleFromJson" : "_doubleFromJson";
                definitionsBuilder.Append($", fromJson: {fromJsonMethod}");
            }

            definitionsBuilder.AppendLine(")");
        }

        private static bool IsA<T>(TypeStatement type)
        {
            return $"{type.Namespace}.{type.Name}" == typeof(T).FullName;
        }

        private void GenerateDefaultConstructor(string name, int level)
        {
            definitionsBuilder
                .AppendSpaces(level + 1)
                .AppendLine($"{name}();");
        }

        private void GenerateToJsonMethod(
            string fullName,
            int level,
            bool includeOverrideAnnotation)
        {
            definitionsBuilder.AppendLine();

            if (includeOverrideAnnotation)
            {
                definitionsBuilder
                   .AppendSpaces(level + 1)
                   .AppendLine("@override");
            }

            definitionsBuilder
                .AppendSpaces(level + 1)
                .Append("Map<String, dynamic> toJson() => ")
                .AppendLine($"_${fullName}ToJson(this);");
        }

        private void GenerateFromJsonConstructor(string name, int level)
        {
            definitionsBuilder.AppendLine()
                .AppendSpaces(level + 1)
                .Append($"factory {name}.fromJson(Map<String, dynamic> json) => ")
                .AppendLine($"_${name}FromJson(json);");
        }

        private void GenerateConstants(List<ConstStatement> constants, int level)
        {
            if (constants.Any())
            {
                definitionsBuilder.AppendLine();

                foreach (var constant in constants)
                {
                    var name = constant.Name.ToCamelCase();

                    definitionsBuilder
                        .AppendSpaces(level + 1)
                        .AppendLine($"static const {name} = {constant.Value};");
                }
            }
        }

        private void GenerateFields(List<FieldStatement> fields, int level)
        {
            if (fields.Any())
            {
                definitionsBuilder.AppendLine();

                foreach (var field in fields)
                {
                    VisitFieldStatement(field, level + 1);
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

            var limit = group.Select(s => s.statement.Namespace.Split('.').Length).Max();

            var depth = 1;

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

        private static string MakeName(string namespaceName, string name, int depth)
        {
            var split = namespaceName.Split('.').Reverse().Take(depth).Append(name);
            return string.Join(string.Empty, split);
        }

        private static string TranslateIdentifier(string identifier)
        {
            var translated = identifier.ToCamelCase();

            if (translated == "new" || translated == "default")
            {
                translated += '_';
            }

            return translated;
        }
    }
}

using System.Collections.Generic;

namespace LeanCode.ContractsGenerator.Languages.Dart
{
    public class DartConfiguration
    {
        public static string[] DefaultPreambleLines { get; set; } = new string[]
        {
            "import 'package:meta/meta.dart';",
            "import 'package:json_annotation/json_annotation.dart';",
            "import 'package:cqrs/cqrs.dart';",
            "part '{0}.g.dart';",
            "abstract class IRemoteQuery<T> extends Query<T> {{}}",
            "abstract class IRemoteCommand extends Command {{}}",
            string.Empty,
        };

        public static string DefaultContractsPreamble => string.Join('\n', DefaultPreambleLines);

        public string[] ContractsPreambleLines { get; set; } = DefaultPreambleLines;

        public string ContractsPreamble
        {
            get => ContractsPreambleLines is null ? null : string.Join('\n', ContractsPreambleLines);
            set => ContractsPreambleLines = value?.Split('\n');
        }

        public string[] UnmangledTypes { get; set; } = new string[]
        {
            "IRemoteQuery",
            "IRemoteCommand",
        };

        public Dictionary<string, string> TypeTranslations { get; set; } = new Dictionary<string, string>
        {
            { "int", "int" },
            { "double", "double" },
            { "float", "double" },
            { "single", "double" },
            { "int32", "int" },
            { "uint32", "int" },
            { "byte", "int" },
            { "sbyte", "int" },
            { "int64", "int" },
            { "short", "int" },
            { "long", "int" },
            { "decimal", "double" },
            { "bool", "bool" },
            { "boolean", "bool" },
            { "datetime", "DateTime" },
            { "timespan", "String" },
            { "datetimeoffset", "String" },
            { "guid", "String" },
            { "string", "String" },
            { "jobject", "dynamic" },
            { "dynamic", "dynamic" },
            { "object", "Object" },
        };
    }
}

using System.Collections.Generic;

namespace LeanCode.ContractsGenerator.Languages.Dart
{
    public class DartConfiguration
    {
        public string[] ContractsPreambleLines { get; set; } = new string[]
        {
            "import 'dart:convert';",
            "import 'package:dart_cqrs/dart_cqrs.dart';",
            "abstract class IRemoteQuery<T1> extends Query<T1> {}",
            "abstract class IRemoteCommand extends Command {}",
            "",
        };

        public string ContractsPreamble
        {
            get => ContractsPreambleLines is null ? null : string.Join('\n', ContractsPreambleLines);
            set => ContractsPreambleLines = value?.Split('\n');
        }

        public string[] UnmangledTypes { get; set; } = new string[]
        {
            "IRemoteQuery",
            "IRemoteCommand"
        };

        public Dictionary<string, string> TypeTranslations = new Dictionary<string, string>
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
            { "datetime", "String" },
            { "timespan", "String" },
            { "datetimeoffset", "String"},
            { "guid", "String" },
            { "string", "String" },
            { "jobject", "dynamic" },
            { "dynamic", "dynamic" },
            { "object", "Object" }
        };
    }
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LeanCode.ContractsGenerator.Languages.Dart
{
    public class DartConfiguration
    {
        public string ContractsPreamble { get; set; } = "abstract class IRemoteQuery<T1, T2> {}\nabstract class IRemoteCommand<T1> {}\n";
        public string ClientPreamble { get; set; } = "";
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


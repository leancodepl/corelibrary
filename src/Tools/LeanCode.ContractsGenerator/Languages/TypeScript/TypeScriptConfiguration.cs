using System.Collections.Generic;

namespace LeanCode.ContractsGenerator.Languages.TypeScript
{
    public class TypeScriptConfiguration
    {
        public string[] ContractsPreambleLines { get; set; } = new string[]
        {
            "/* eslint-disable */",
            "import { ClientType } from \"@leancode/cqrs-client/ClientType\";",
            "import { CQRS } from \"@leancode/cqrs-client/CQRS\";",
            "import { IRemoteCommand, IRemoteQuery } from \"@leancode/cqrs-client/ClientType\";",
            string.Empty,
        };

        public string ContractsPreamble
        {
            get => ContractsPreambleLines is null ? null : string.Join('\n', ContractsPreambleLines);
            set => ContractsPreambleLines = value?.Split('\n');
        }

        public Dictionary<string, string> TypeTranslations { get; set; } = new Dictionary<string, string>
        {
            { "int", "number" },
            { "double", "number" },
            { "float", "number" },
            { "single", "number" },
            { "int32", "number" },
            { "uint32", "number" },
            { "byte", "number" },
            { "sbyte", "number" },
            { "int64", "number" },
            { "short", "number" },
            { "long", "number" },
            { "decimal", "number" },
            { "bool", "boolean" },
            { "boolean", "boolean" },
            { "datetime", "string" },
            { "timespan", "string" },
            { "datetimeoffset", "string" },
            { "guid", "string" },
            { "string", "string" },
            { "jobject", "any" },
            { "dynamic", "any" },
            { "object", "any" },
        };
    }
}

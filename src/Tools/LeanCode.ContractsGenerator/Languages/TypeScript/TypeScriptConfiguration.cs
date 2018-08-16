using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LeanCode.ContractsGenerator.Languages.TypeScript
{
    public class TypeScriptConfiguration
    {
        public string ContractsPreamble { get; set; } = "type IRemoteCommand<TContext> = import(\"@leancode/cqrs-client/ClientType\").IRemoteCommand<TContext>\ntype IRemoteQuery<TContext, TOutput> = import(\"@leancode/cqrs-client/ClientType\").IRemoteQuery<TContext, TOutput>\n";
        public string ClientPreamble { get; set; } = "import { CQRS } from \"@leancode/cqrs-client/CQRS\";\nimport { ClientType } from \"@leancode/cqrs-client/ClientType\";\n";
        public Dictionary<string, string> TypeTranslations = new Dictionary<string, string>
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
            { "datetimeoffset", "string"},
            { "guid", "string" },
            { "string", "string" },
            { "jobject", "any" },
            { "dynamic", "any" },
            { "object", "any" }
        };
    }
}


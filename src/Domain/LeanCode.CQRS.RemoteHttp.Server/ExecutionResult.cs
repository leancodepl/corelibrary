using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    internal struct ExecutionResult
    {
        public ExecutionStatus Status { get; private set; }
        public int StatusCode { get; private set; }
        public object Payload { get; private set; }

        public bool Skipped => Status == ExecutionStatus.Skip;
        public bool Failed => Status == ExecutionStatus.Failed;
        public bool Succeeded => Status == ExecutionStatus.Succeeded;

        public static ExecutionResult Skip() => new ExecutionResult
        {
            Status = ExecutionStatus.Skip,
        };

        public static ExecutionResult Fail(int code) => new ExecutionResult
        {
            Status = ExecutionStatus.Failed,
            StatusCode = code,
            Payload = null,
        };

        public static ExecutionResult Success(object payload, int code = 200) => new ExecutionResult
        {
            Status = ExecutionStatus.Succeeded,
            StatusCode = code,
            Payload = payload,
        };

        public enum ExecutionStatus
        {
            Skip,
            Failed,
            Succeeded,
        }
    }
}

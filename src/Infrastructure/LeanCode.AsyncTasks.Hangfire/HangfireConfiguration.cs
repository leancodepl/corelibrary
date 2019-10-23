using System;
using Hangfire;
using Hangfire.SqlServer;

namespace LeanCode.AsyncTasks.Hangfire
{
    public class HangfireConfiguration
    {
        public const string DefaultQueue = "default";

        public string Name { get; }
        public string ConnectionString { get; }
        public string Schema { get; }

        public string Queue { get; }
        public Action<BackgroundJobServerOptions>? ServerConfig { get; }
        public Action<SqlServerStorageOptions>? StorageConfig { get; }

        public int InitializationOrder { get; }

        public HangfireConfiguration(
            string name,
            string connectionString,
            string schema,
            string queue,
            Action<BackgroundJobServerOptions>? serverConfig,
            Action<SqlServerStorageOptions>? storageConfig,
            int initOrder)
        {
            Name = name;
            ConnectionString = connectionString;
            Schema = schema;
            Queue = queue;
            ServerConfig = serverConfig;
            StorageConfig = storageConfig;
            InitializationOrder = initOrder;
        }

        public HangfireConfiguration(
            string name,
            string connectionString,
            string schema)
        {
            Name = name;
            ConnectionString = connectionString;
            Schema = schema;
            Queue = DefaultQueue;
            ServerConfig = null;
            StorageConfig = null;
            InitializationOrder = int.MaxValue;
        }
    }
}

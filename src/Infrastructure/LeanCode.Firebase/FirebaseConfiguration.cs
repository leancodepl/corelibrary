using System.Text.Json.Nodes;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace LeanCode.Firebase
{
    public static class FirebaseConfiguration
    {
        private const string DefaultAppName = "[DEFAULT]";

        public static FirebaseApp Prepare(string? cfg) => Prepare(cfg, DefaultAppName);

        public static FirebaseApp Prepare(string? cfg, string name)
        {
            if (string.IsNullOrEmpty(cfg))
            {
                return FirebaseApp.Create(
                    new AppOptions
                    {
                        Credential = GoogleCredential.FromAccessToken("owner"),
                        ProjectId = "default-project-id",
                    },
                    name);
            }
            else
            {
                return FirebaseApp.Create(
                    new AppOptions
                    {
                        Credential = GoogleCredential.FromJson(cfg),
                        ProjectId = JsonNode.Parse(cfg)?["project_id"]?.ToString(),
                    },
                    name);
            }
        }
    }
}

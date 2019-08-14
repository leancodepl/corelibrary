using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;

namespace LeanCode.Firestore
{
    public class FirestoreConfiguration
    {
        public static FirebaseApp Prepare(string cfg)
        {
            if (string.IsNullOrEmpty(cfg))
            {
                return FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromAccessToken("STUB"),
                    ProjectId = "STUB",
                });
            }
            else
            {
                var projectId = JObject.Parse(cfg)["project_id"].Value<string>();
                return FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromJson(cfg),
                    ProjectId = projectId,
                });
            }
        }
    }
}

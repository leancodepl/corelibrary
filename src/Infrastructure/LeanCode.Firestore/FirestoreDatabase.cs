using System;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using Grpc.Core;
using LeanCode.AsyncInitializer;

namespace LeanCode.Firestore
{
    public class FirestoreDatabase : IAsyncInitializable
    {
        private readonly Channel channel;
        private readonly FirestoreClient client;

        public FirestoreDb Database { get; }

        public FirestoreDatabase(FirebaseApp firebaseApp)
        {
            var credentials = firebaseApp.Options.Credential;

            channel = new Channel(
               FirestoreClient.DefaultEndpoint.Host,
               FirestoreClient.DefaultEndpoint.Port,
               credentials.ToChannelCredentials());

            client = FirestoreClient.Create(channel);

            Database = FirestoreDb.Create(firebaseApp.Options.ProjectId, client);
        }

        int IAsyncInitializable.Order => 0;
        Task IAsyncInitializable.InitializeAsync() => Task.CompletedTask;
        Task IAsyncInitializable.DeinitializeAsync() => channel.ShutdownAsync();
    }
}

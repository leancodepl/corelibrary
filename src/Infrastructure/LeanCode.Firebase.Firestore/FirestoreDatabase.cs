using System;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using Grpc.Core;
using LeanCode.AsyncInitializer;

namespace LeanCode.Firebase.Firestore
{
    public class FirestoreDatabase : IAsyncInitializable
    {
        private readonly FirebaseApp firebaseApp;
        private FirestoreDb? database;

        public FirestoreDb Database => database ?? throw new InvalidOperationException("The database needs to be initialized first.");

        public FirestoreDatabase(FirebaseApp firebaseApp)
        {
            this.firebaseApp = firebaseApp;
        }

        async Task IAsyncInitializable.InitializeAsync()
        {
            var credentials = firebaseApp.Options.Credential;

            var builder = new FirestoreClientBuilder
            {
                ChannelCredentials = credentials.ToChannelCredentials(),
            };
            var client = await builder.BuildAsync();
            database = FirestoreDb.Create(firebaseApp.Options.ProjectId, client);
        }

        int IAsyncInitializable.Order => 0;
        Task IAsyncInitializable.DeinitializeAsync() => Task.CompletedTask;
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Api.Gax;
using Google.Cloud.Firestore;
using Grpc.Auth;
using Microsoft.Extensions.Hosting;

namespace LeanCode.Firebase.Firestore
{
    public class FirestoreDatabase : IHostedService
    {
        private readonly FirebaseApp firebaseApp;
        private FirestoreDb? database;

        public FirestoreDb Database => database ?? throw new InvalidOperationException("The database needs to be initialized first.");

        public FirestoreDatabase(FirebaseApp firebaseApp)
        {
            this.firebaseApp = firebaseApp;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            database = await new FirestoreDbBuilder
            {
                ChannelCredentials = firebaseApp.Options.Credential.ToChannelCredentials(),
                ProjectId = firebaseApp.Options.ProjectId ?? (await Platform.InstanceAsync()).ProjectId,
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
            }.BuildAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

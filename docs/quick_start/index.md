# Quick start

LeanCode CoreLibrary offers flexibility to suit different needs. Whether you want quick results or a detailed dive into features, it's here for you. In this tutorial, we'll take the DIY route, building, setting up CoreLibrary based project from scratch and learning how to deploy it on a local Kubernetes cluster.

!!! Prerequisites info
    Prior to proceeding with this tutorial, ensure you have the following requirements installed: [kubectl](https://kubernetes.io/docs/tasks/tools/),
    [K3D](https://k3d.io/#installation), [Helm](https://helm.sh/docs/intro/quickstart/), [Tilt](https://docs.tilt.dev/install.html), [Terraform](https://www.terraform.io/), [dotnet](https://dotnet.microsoft.com/en-us/download), [git](https://git-scm.com/).

## Setting up the template

To get started, begin by cloning the repository containing `lncdproj` template:

```sh
git clone git@github.com:leancodepl/exampleapp.git
```

Once the repository is cloned, navigate to the `exampleapp` directory. Here, install the `lncdproj` .NET template:

```sh
dotnet new install .
```

Then, you can run it in your preferred directory using the `dotnet new lncdproj` command. The `lncdproj` template requires two arguments: `--project-name`, specifying the name of the newly created project, and `--context`, indicating the name of the first bounded context used throughout the project (additional `--allow-scripts` flag is used to not display user prompts):

```sh
dotnet new lncdproj --allow-scripts Yes --project-name ProjectName --context ContextName
```

This template generates 3 directories:

- `backend`: Contains the dotnet solution with template files.
- `dev-cluster`: Holds the cluster configuration for local development.
- `infrastructure`: Contains the configuration for infrastructure in production environments.

## Setting up the local cluster

Let's set up a local Kubernetes cluster, which is a vital part of our development environment. The `dev-cluster` directory contains configuration files for various services that will be deployed to our local Kubernetes cluster. These services include:

- **PostgreSQL Database**: A relational database system for storing and managing application data.
- **Blob Storage**: A service for storing large amounts of unstructured data, like images or videos.
- **RabbitMQ**: A message broker that enables applications to communicate with each other asynchronously.
- **Kratos**: An identity management system that handles user accounts, authentication, and authorization.
- **Seq**: Log system designed for searching, analysis, and visualization of structured log data.
- **Jaeger**: An open-source tool for tracing requests.
- **Traefik**: A cloud-native application proxy that routes incoming requests to the appropriate microservices.
- **OpenTelemetry**: A set of APIs, SDKs, and tools to generate, collect, and export telemetry data.
- **Metabase**: Tool for visualizing and sharing data insights.

To configure the local cluster, start by navigating to the `dev-cluster` directory. Once there, ensure that you've set the `sendgrid_api_key` in the `terraform.tfvars` file. Without this api key, the functionality for sending emails won't be available:

```terraform
sendgrid_api_key = "my_sendgrid_api_key"
```

Then, execute the following script to set up the local cluster:

```sh
./deploy.sh
```

## Setting up the application

After successfully setting up the local cluster, your next step is to initiate your application. Begin by navigating to the `backend` directory. This directory is the heart of your application, containing various subdirectories and files, each serving a specific purpose:

```txt
backend
├── dev - Configuration for local development.
├── release - Dockerfiles for production deployment.
├── src
│   ├── Apps
│   │   ├── ProjectName.Api - Hosts the API, serving as the backend entrypoint.
│   │   ├── ProjectName.LeanPipeFunnel - Manages real-time notifications.
│   │   └── ProjectName.Migrations - Handles database migrations.
│   └── ContextName
│       ├── ProjectName.ContextName.Contracts - API client interfaces.
│       ├── ProjectName.ContextName.Domain - Core business logic.
│       └── ProjectName.ContextName.Services - Service layer.
├── tests
│   ├── ContextName
│   │   ├── ProjectName.ContextName.Domain.Tests - Tests for business logic.
│   │   └── ProjectName.ContextName.Services.Tests - Tests for service layer.
│   ├── ProjectName.IntegrationTests - Comprehensive application-wide tests.
```

### Generating initial migrations

Let's start with generating initial migrations. Begin by navigating, to the `backend/src/Apps/ProjectName.Migrations` directory. Generate initial migrations using the `dotnet ef` tool (install it via `dotnet tool install --global dotnet-ef`):

```sh
# It does not need to point to a real database
export PostgreSQL__ConnectionString='Host=localhost;Database=app;Username=app;Password=Passw12#'
dotnet ef migrations add --context ContextNameDbContext -o Migrations InitialMigration
```

### Starting the application

Once initial migrations have been generated we can start the application. First of all ensure that you've selected the `kubectl` context corresponding to the newly created cluster:

```sh
kubectl config use-context k3d-projectname
```

Then you can run the migrations and the API using the following command:

```sh
tilt up migrations api
```

Once Tilt applies migrations and starts the API it should be available at: <https://projectname.local.lncd.pl/api>. You can also run the integration tests using following command:

```sh
tilt up integration_tests
```

## Troubleshooting

- If you encounter a `Permission denied` error while running `tilt up` commands for any `.sh` files, attempt to resolve it by granting execute permission using the `chmod +x` command and restarting tilt using `tilt down` and `tilt up` commands.
- If the `deploy.sh` script fails during the local cluster creation process, you can address the issue by either applying missing services with the `terraform apply -auto-approve` command or by re-running the `deploy.sh` script to redeploy the cluster.

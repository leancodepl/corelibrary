# Quick start

LeanCode CoreLibrary offers flexibility to suit different needs. Whether you want quick results or a detailed dive into features, it's here for you. In this tutorial, we'll take the DIY route, building, setting up CoreLibrary based project from scratch and learning how to deploy it on a local Kubernetes cluster.

!!! Prerequisites info
    Prior to proceeding with this tutorial, ensure you have the following requirements installed: [kubectl](https://kubernetes.io/docs/tasks/tools/),
    [K3D](https://k3d.io/#installation), [Helm](https://helm.sh/docs/intro/quickstart/), [Tilt](https://docs.tilt.dev/install.html), [Terraform](https://www.terraform.io/), [dotnet](https://dotnet.microsoft.com/en-us/download), [git](https://git-scm.com/).

## Setting up the template

The `lncdproj` template, provided by LeanCode CoreLibrary, is a comprehensive template designed for modern .NET application development, featuring CQRS as client-facing APIs, PostgreSQL for database management, and Kratos for identity management. It integrates with MassTransit for efficient communication and includes tools to aid testing. Additionally, the template incorporates LeanPipe for real-time SignalR notifications, and Blob Storage for managing large data, collectively laying a solid foundation for building efficient .NET applications.

To get started, begin by cloning the repository containing `lncdproj` template:

```sh
git clone git@github.com:leancodepl/exampleapp.git
```

Once the repository is cloned, navigate to the `exampleapp` directory. Here, install the `lncdproj` .NET template:

```sh
dotnet new install .
```

Then, you can run it in your preferred directory using the `dotnet new lncdproj` command with following arguments:

- `--project-name` - name of the project,
- `--context` - name of the first bounded context,
- `--allow-scripts` - optional argument used to suppress user prompts from being displayed.

```sh
dotnet new lncdproj --project-name ProjectName --context ContextName --allow-scripts Yes
```

This template generates 3 directories:

- `backend`: Contains the dotnet solution with template files.
- `dev-cluster`: Holds the cluster configuration for local development.
- `infrastructure`: Contains the configuration for infrastructure in production environments.

Let's inspect the `backend` directory. This directory is the heart of your application, containing various subdirectories and files, each serving a specific purpose:

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

## Setting up the cluster

In our development process, we emphasize creating an environment that mirrors production as closely as possible. This approach ensures consistency and reliability across different stages of development, testing, and deployment. To achieve this, we utilize a local Kubernetes cluster, which allows us to simulate the behavior of our applications in a controlled environment that resembles our production setup. Our local Kubernetes cluster, configured in the `dev-cluster` directory, comprises several key services:

- **PostgreSQL Database**: A relational database system for storing and managing application data.
- **Blob Storage**: A service for storing large amounts of unstructured data, like images or videos.
- **RabbitMQ**: A message broker that enables applications to communicate with each other asynchronously.
- **Kratos**: An identity management system that handles user accounts, authentication, and authorization.
- **Seq**: Log system designed for searching, analysis, and visualization of structured log data.
- **Jaeger**: An open-source tool for tracing requests.
- **Traefik**: A cloud-native application proxy that routes incoming requests to the appropriate microservices.
- **OpenTelemetry**: A set of APIs, SDKs, and tools to generate, collect, and export telemetry data.

To configure the local cluster, start by navigating to the `dev-cluster` directory. Once there, you can optionally set the `sendgrid_api_key` in the `terraform.tfvars` file:

```terraform
sendgrid_api_key = "my_sendgrid_api_key"
```

Follow these commands to establish your local cluster, or alternatively, use the `deploy.sh` script for an automated setup:

```sh
# Add and update the Helm repository for Traefik.
helm repo add traefik https://helm.traefik.io/traefik
helm repo update

# Apply cluster configuration.
terraform init
terraform apply -target data.local_file.kubeconfig -auto-approve
terraform apply -target helm_release.traefik -auto-approve
terraform apply -auto-approve
```

## Generating initial migrations

Once local cluster has been deployed we can generate initial migrations. Begin by navigating, to the `backend/src/Apps/ProjectName.Migrations` directory. Generate initial migrations using the `dotnet ef` tool (install it via `dotnet tool install --global dotnet-ef`):

```sh
# It does not need to point to a real database
export PostgreSQL__ConnectionString='Host=localhost;Database=app;Username=app;Password=Passw12#'
dotnet ef migrations add --context ContextNameDbContext -o Migrations InitialMigration
```

## Starting the application

Once initial migrations have been generated we can start the application. Let's navigate to the `backend` directory and ensure that the `kubectl` context corresponding to the newly created cluster is selected:

```sh
kubectl config use-context k3d-projectname
```

To execute the migrations, use the following command:

```sh
tilt up migrations
```

With the migrations applied, you can then initiate the API using:

```sh
tilt up api
```

Once Tilt starts the API it should be available at: <https://projectname.local.lncd.pl/api>. You can also run the integration tests using following command:

```sh
tilt up integration_tests
```

## Troubleshooting

- If the `deploy.sh` script fails during the local cluster creation process, you can address the issue by either applying missing services with the `terraform apply -auto-approve` command or by re-running the `deploy.sh` script to redeploy the cluster.

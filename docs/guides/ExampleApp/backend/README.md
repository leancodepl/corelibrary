# ExampleApp

>
> TODO: put here some meaningful quote
>

## How to bootstrap the project

### 1. Generate self-signed certificates for the proxy:

```sh
cd dev/proxy
./generate_certs.sh
```

### 2. Generate initial migrations

```sh
cd src/Apps/ExampleApp.Migrations
# This is required for now, but it does not need to point to a real database
export SqlServer__ConnectionString='Server=localhost,1433;Database=App;User Id=sa;Password=yourStrong(!)Password'
dotnet ef migrations add --context CoreDbContext -o Core InitialMigration # Our context
dotnet ef migrations add --context PersistedGrantDbContext -o Auth InitialMigration # IdentityServer4
```

### 3. Trust the certificate (Ubuntu)

```sh
cp dev/proxy/CA.pem /usr/local/share/ca-certificates/CA.crt
update-ca-certificates
```

### 4. Trust the certificate in Chrome

Chrome uses a separate certificate store. To add certificate in Chrome:

1. Go to chrome://settings/certificates .
2. Import dev/proxy/CA.pem file in the Authorities section.
3. Select "Trust this certificate for identifying websites".

### 5. Setup the development environment cluster

Related instructions are in the [dev-cluster](../dev-cluster/README.md).

### 5. Migrate the database and start the app

```sh
tilt up migrations api
```

Or run the integration tests:

```sh
tilt up integration_tests
```

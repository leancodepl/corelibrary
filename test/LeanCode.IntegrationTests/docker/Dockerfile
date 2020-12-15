FROM leancode.azurecr.io/dotnet:rootless-5.0

COPY Docker.Directory.Build.props /app/Directory.Build.props

# Wait for SQL Server first
WORKDIR /app/code/test/LeanCode.IntegrationTests
ENV ASPNETCORE_TEST_CONTENTROOT_LEANCODE_INTEGRATIONTESTS=/app/code/test/LeanCode.IntegrationTests
ENTRYPOINT sleep 10 && dotnet test

FROM leancode.azurecr.io/dotnet:rootless-5.0

COPY Docker.Directory.Build.props /app/Directory.Build.props

# Wait for SQL Server first
WORKDIR /app/code/test/Testing/LeanCode.IntegrationTestHelpers.Tests
ENV ASPNETCORE_TEST_CONTENTROOT_LEANCODE_INTEGRATIONTESTHELPERS_TESTS=/app/code/test/Testing/LeanCode.IntegrationTestHelpers.Tests
ENTRYPOINT sleep 10 && dotnet test

FROM mcr.microsoft.com/dotnet/sdk:9.0
COPY Docker.Directory.Build.props /app/Directory.Build.props

# Wait for SQL Server first
WORKDIR /app/code/test/LeanCode.IntegrationTests
ENV ASPNETCORE_TEST_CONTENTROOT_LEANCODE_INTEGRATIONTESTS=/app/code/test/LeanCode.IntegrationTests
ENTRYPOINT sleep 10 && dotnet test

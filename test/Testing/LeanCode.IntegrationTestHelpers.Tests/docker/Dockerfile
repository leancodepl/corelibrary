FROM leancode.azurecr.io/dotnet:3.1

WORKDIR /app

COPY . .

RUN dotnet restore

WORKDIR /app/test/Testing/LeanCode.IntegrationTestHelpers.Tests
ENV ASPNETCORE_TEST_CONTENTROOT_LEANCODE_INTEGRATIONTESTHELPERS_TESTS=/app/code/test/Testing/LeanCode.IntegrationTestHelpers.Tests
RUN dotnet build --no-restore

# Wait for SQL Server first
ENTRYPOINT sleep 10 && dotnet test

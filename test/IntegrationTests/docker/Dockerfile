FROM leancode.azurecr.io/dotnet:5.0

WORKDIR /app/code

COPY . .

RUN dotnet restore

WORKDIR /app/code/test/IntegrationTests
ENV ASPNETCORE_TEST_CONTENTROOT_LEANCODE_INTEGRATIONTESTS=/app/code/test/IntegrationTests
RUN dotnet build --no-restore

# Wait for SQL Server first
ENTRYPOINT sleep 10 && dotnet test

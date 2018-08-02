FROM leancode.azurecr.io/dotnet:2.1-sdk-alpine

WORKDIR /app

COPY . .

RUN dotnet restore

WORKDIR /app/test/Testing/LeanCode.IntegrationTestHelpers.Tests
RUN dotnet build --no-restore

ENTRYPOINT sleep 5 && dotnet test

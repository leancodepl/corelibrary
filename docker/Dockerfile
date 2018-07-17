FROM leancode.azurecr.io/dotnet:2.1-sdk-alpine

WORKDIR /app

COPY CHANGELOG.md .
COPY build.fsx .
COPY build.fsx.lock .

RUN fake run --fsiargs "--define:BOOTSTRAP" build.fsx

COPY . .

ENTRYPOINT fake run build.fsx -t PublishToMyGet

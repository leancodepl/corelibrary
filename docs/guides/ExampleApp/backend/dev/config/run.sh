#!/usr/bin/env bash

source /app/config/config.sh
exec -a ExampleApp.Api dotnet /app/bin/ExampleApp.Api.dll

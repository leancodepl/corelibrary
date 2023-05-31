#!/usr/bin/env bash
source /app/config/config.sh

cd /app/bin
if [ -z "$SEED" ]; then
    echo "Migrating"
    dotnet ExampleApp.Migrations.dll migrate
else
    echo "Seeding"
    dotnet ExampleApp.Migrations.dll seed
fi

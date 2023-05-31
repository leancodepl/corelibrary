#!/usr/bin/env bash

export Domains__Public='exampleapp.local.lncd.pl'
export Domains__ApiInternal='exampleapp.local.lncd.pl'

export Logging__EnableDetailedInternalLogs=true
export Logging__MinimumLevel=Verbose
export Logging__SeqEndpoint='http://seq-svc.shared.svc.cluster.local'

export SqlServer__ConnectionString='Server=mssql-svc.shared.svc.cluster.local,1433;Database=App;User Id=sa;Password=Passw12#;Connection Timeout=5;Encrypt=false'
export BlobStorage__ConnectionString='DefaultEndpointsProtocol=http;AccountName=blobstorage;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://blobstorage.blob.svc.cluster.local/;'

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ -f "$DIR/secrets.sh" ]]
then
    source "$DIR/secrets.sh"
fi

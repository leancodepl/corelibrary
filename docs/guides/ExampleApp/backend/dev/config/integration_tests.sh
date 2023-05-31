#!/usr/bin/env bash

export Domains__Public=localhost
export Domains__ApiInternal=localhost

#TODO: Fix once corelib is fixed
export SqlServer__ConnectionStringBase='Server=mssql-svc.shared.svc.cluster.local,1433;User Id=sa;Password=Passw12#;Encrypt=false'
export BlobStorage__ConnectionString='DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://storage.local.lncd.pl:10000/devstoreaccount1;'

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ -f "$DIR/integration_tests_secrets.sh" ]]
then
    source "$DIR/integration_tests_secrets.sh"
fi

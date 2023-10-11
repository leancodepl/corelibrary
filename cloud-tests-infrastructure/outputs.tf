output "test_envs" {
  sensitive = true
  value     = <<-EOT
CORELIB_TESTS_TENANT_ID="${azuread_service_principal.tests.application_tenant_id}"
CORELIB_TESTS_CLIENT_ID="${azuread_application.tests.application_id}"
CORELIB_TESTS_CLIENT_SECRET="${azuread_service_principal_password.tests.value}"
CORELIB_TESTS_NPGSQL_CONNECTION_STRING='${local.npg_connection_string}'
CORELIB_TESTS_AZURE_BLOB_STORAGE_SERVICE_URI='${module.storage.storage_blob_endpoint}'
CORELIB_TESTS_AZURE_TABLE_STORAGE_SERVICE_URI='${module.storage.storage_blob_endpoint}'
CORELIB_TESTS_AZURE_BLOB_STORAGE_CONTAINER_NAME='${local.blob_storage_container_name}'
CORELIB_TESTS_AZURE_TABLE_STORAGE_TABLE_NAME='${local.table_storage_table_name}'
EOT
}

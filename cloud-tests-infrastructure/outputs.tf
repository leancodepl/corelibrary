output "test_envs" {
  sensitive = true
  value     = <<-EOT
CORELIB_TESTS_TENANT_ID="${azuread_service_principal.tests.application_tenant_id}"
CORELIB_TESTS_CLIENT_ID="${azuread_application.tests.application_id}"
CORELIB_TESTS_CLIENT_SECRET="${azuread_service_principal_password.tests.value}"
CORELIB_TESTS_NPGSQL_CONNECTION_STRING='${local.npg_connection_string}'
EOT
}

resource "azurerm_postgresql_flexible_server" "server" {
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.location

  name       = var.postgres_server_name
  version    = "14"
  sku_name   = "B_Standard_B1ms"
  storage_mb = "32768"

  authentication {
    active_directory_auth_enabled = true
    password_auth_enabled         = false
    tenant_id                     = var.tenant_id
  }

  lifecycle {
    ignore_changes = [zone]
  }
}

resource "azurerm_postgresql_flexible_server_active_directory_administrator" "sp" {
  resource_group_name = azurerm_postgresql_flexible_server.server.resource_group_name
  server_name         = azurerm_postgresql_flexible_server.server.name

  tenant_id      = var.tenant_id
  object_id      = azuread_service_principal.tests.object_id
  principal_name = azuread_service_principal.tests.display_name
  principal_type = "ServicePrincipal"
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_all" {
  server_id = azurerm_postgresql_flexible_server.server.id
  name      = "allow-all"

  start_ip_address = "0.0.0.0"
  end_ip_address   = "255.255.255.255"
}

resource "azurerm_postgresql_flexible_server_database" "database" {
  server_id = azurerm_postgresql_flexible_server.server.id

  name      = "corelibrary-tests"
  charset   = "UTF8"
  collation = "en_US.utf8"
}

locals {
  npg_connection_string = join("", [
    "Host=${azurerm_postgresql_flexible_server.server.fqdn};SSL Mode=VerifyFull;",
    "Database=${azurerm_postgresql_flexible_server_database.database.name};",
    "Username=${azurerm_postgresql_flexible_server_active_directory_administrator.sp.principal_name};",
  ])
}

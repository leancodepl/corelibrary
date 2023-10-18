resource "azurerm_storage_account" "storage" {
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  name                = var.storage_name

  account_kind             = "StorageV2"
  account_replication_type = "LRS"
  account_tier             = "Standard"
  access_tier              = "Hot"

  allow_nested_items_to_be_public = true
  enable_https_traffic_only       = true

  min_tls_version = "TLS1_2"
}

resource "azurerm_storage_container" "container" {

  storage_account_name = azurerm_storage_account.storage.name

  name                  = local.blob_storage_container_name
  container_access_type = "private"
}

resource "azurerm_role_assignment" "table_data_contributor" {
  scope                = azurerm_storage_account.storage.id
  role_definition_name = "Storage Table Data Contributor"
  principal_id         = azuread_service_principal.tests.object_id
}

resource "azurerm_role_assignment" "blob_data_owner" {
  scope                = azurerm_storage_account.storage.id
  role_definition_name = "Storage Blob Data Owner"
  principal_id         = azuread_service_principal.tests.object_id
}

resource "azurerm_storage_table" "example" {
  name                 = local.table_storage_table_name
  storage_account_name = azurerm_storage_account.storage.name
}

locals {
  blob_storage_container_name = "audit-logs"
  table_storage_table_name    = "auditlogs"
}

module "storage" {
  source = "git@github.com:leancodepl/terraform-common-modules.git//azure_blob_storage?ref=v0.1.0"

  resource_group_name  = azurerm_resource_group.rg.name
  storage_account_name = var.storage_name

  data_owners_object_ids = { tests = azuread_service_principal.tests.object_id }

  blob_containers = {
    (local.blob_storage_container_name) = {
      access_type = "private"
    }
  }

  blob_cors_rules = []
  tags            = {}
}

resource "azurerm_storage_table" "example" {
  name                 = local.table_storage_table_name
  storage_account_name = module.storage.storage_account_name
}

locals {
  blob_storage_container_name = "audit-logs"
  table_storage_table_name    = "auditlogs"
}

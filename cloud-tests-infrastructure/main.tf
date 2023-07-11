terraform {
  required_version = ">= 1.0.2"

  backend "azurerm" {}

  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.39"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.58"
    }
  }
}

provider "azurerm" {
  tenant_id       = var.tenant_id
  subscription_id = var.subscription_id

  features {}
}

provider "azuread" {
  tenant_id = var.tenant_id
}

resource "azurerm_resource_group" "rg" {
  name     = var.resource_group_name
  location = var.location
}

resource "azuread_application" "tests" {
  display_name    = "Core Library Tests"
  identifier_uris = ["https://corelibrary-tests.project.lncd.pl"]
}

resource "azuread_service_principal" "tests" {
  application_id = azuread_application.tests.application_id
}

resource "azuread_service_principal_password" "tests" {
  service_principal_id = azuread_service_principal.tests.id
}

terraform {
  required_version = ">= 1.0.2"

  backend "azurerm" {}

  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.12"
    }
    time = {
      source  = "hashicorp/time"
      version = "0.13.1"
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
  client_id = azuread_application.tests.client_id
}


resource "time_rotating" "sp_secret_rotation" {
  rotation_years = 1
}

resource "azuread_service_principal_password" "tests" {
  service_principal_id = azuread_service_principal.tests.id
  rotate_when_changed = {
    time = time_rotating.sp_secret_rotation.id
  }
}

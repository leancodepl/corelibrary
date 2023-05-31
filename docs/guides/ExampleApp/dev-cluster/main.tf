terraform {
  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "2.7.1"
    }
    docker = {
      source  = "kreuzwerker/docker"
      version = "2.16.0"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "2.4.1"
    }
  }
}


provider "kubernetes" {
  host                   = local.credentials.host
  client_certificate     = local.credentials.client_certificate
  client_key             = local.credentials.client_key
  cluster_ca_certificate = local.credentials.cluster_ca_certificate

  experiments {
    manifest_resource = true
  }
}

provider "helm" {
  kubernetes {
    host                   = local.credentials.host
    client_certificate     = local.credentials.client_certificate
    client_key             = local.credentials.client_key
    cluster_ca_certificate = local.credentials.cluster_ca_certificate
  }
}

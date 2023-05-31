locals {
  k8s_shared_namespace = kubernetes_namespace.shared.metadata[0].name
  k8s_main_namespace   = kubernetes_namespace.main.metadata[0].name
  k8s_blob_namespace   = kubernetes_namespace.blob.metadata[0].name
}

resource "kubernetes_namespace" "shared" {
  metadata {
    name = "shared"
  }
}

resource "kubernetes_namespace" "main" {
  metadata {
    name = "exampleapp-dev"
  }
}

resource "kubernetes_namespace" "blob" {
  metadata {
    name = "blob"
  }
}

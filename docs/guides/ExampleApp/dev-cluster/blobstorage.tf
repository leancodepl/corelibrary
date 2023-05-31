resource "kubernetes_deployment_v1" "blobstorage_deployment" {
  metadata {
    name      = "blobstorage"
    namespace = local.k8s_blob_namespace
    labels = {
      app = "blobstorage"
    }
  }
  spec {
    replicas = 1
    selector {
      match_labels = {
        app = "blobstorage"
      }
    }
    template {
      metadata {
        labels = {
          app = "blobstorage"
        }
      }
      spec {
        container {
          command = [
            "azurite",
            "--skipApiVersionCheck",
            "--blobHost",
            "0.0.0.0",
            "-d",
            "/var/debug.log"
          ]
          env {
            name  = "executable"
            value = "blob"
          }
          env {
            name  = "AZURITE_ACCOUNTS"
            value = "blobstorage:Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="
          }
          image = "mcr.microsoft.com/azure-storage/azurite:3.16.0"
          name  = "blobstorage"
          port {
            container_port = 10000
          }
          volume_mount {
            name       = "data"
            mount_path = "/data"
          }
        }
        volume {
          name = "data"
          persistent_volume_claim {
            claim_name = kubernetes_manifest.blobstorage_pvc.manifest.metadata.name
          }
        }
      }
    }
  }
}

resource "kubernetes_manifest" "blobstorage_pvc" {
  manifest = {
    "apiVersion" = "v1"
    "kind"       = "PersistentVolumeClaim"
    "metadata" = {
      "name"      = "blobstorage-pvc"
      "namespace" = local.k8s_blob_namespace
      "labels" = {
        "app" = "blobstorage"
      }
    }
    "spec" = {
      "accessModes" = [
        "ReadWriteOnce",
      ]
      "resources" = {
        "requests" = {
          "storage" = "10Gi"
        }
      }
    }
  }
}

resource "kubernetes_service_v1" "blobstorage_service" {
  metadata {
    # keep the name in sync with AZURITE_ACCOUNTS, otherwise you get 400 (Invalid storage account.) when calling storage
    # using cluster local network
    name      = "blobstorage"
    namespace = local.k8s_blob_namespace
    labels = {
      app = "blobstorage"
    }
  }
  spec {
    type = "ClusterIP"
    port {
      port        = 80
      target_port = 10000
    }
    selector = {
      app = "blobstorage"
    }
  }
}

resource "kubernetes_ingress_v1" "blobstorage_ingress" {
  metadata {
    name      = "blobstorage-ingress"
    namespace = local.k8s_blob_namespace
    labels = {
      app = "blobstorage"
    }
  }
  spec {
    rule {
      host = "blobstorage.local.lncd.pl"
      http {
        path {
          backend {
            service {
              name = kubernetes_service_v1.blobstorage_service.metadata[0].name
              port {
                number = 80
              }
            }
          }
          path_type = "ImplementationSpecific"
        }
      }
    }
  }
}

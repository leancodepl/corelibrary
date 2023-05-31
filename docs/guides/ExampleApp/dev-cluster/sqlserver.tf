resource "kubernetes_deployment_v1" "mssql_deployment" {
  metadata {
    name      = "mssql"
    namespace = local.k8s_shared_namespace
    labels = {
      app = "mssql"
    }
  }
  spec {
    selector {
      match_labels = {
        app = "mssql"
      }
    }
    template {
      metadata {
        labels = {
          app = "mssql"
        }
      }
      spec {
        container {
          env {
            name  = "ACCEPT_EULA"
            value = "Y"
          }
          env {
            name  = "MSSQL_SA_PASSWORD"
            value = "Passw12#"
          }
          image = "mcr.microsoft.com/mssql/server:2022-latest"
          name  = "mssql"
          volume_mount {
            mount_path = "/var/opt/mssql"
            name       = "data"
          }
        }
        volume {
          name = "data"
          persistent_volume_claim {
            claim_name = kubernetes_manifest.mssql_pvc.manifest.metadata.name
          }
        }
      }
    }
  }
}

resource "kubernetes_manifest" "mssql_pvc" {
  manifest = {
    "apiVersion" = "v1"
    "kind"       = "PersistentVolumeClaim"
    "metadata" = {
      "name"      = "mssql-pvc"
      "namespace" = local.k8s_shared_namespace
      "labels" = {
        "app" = "mssql"
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

resource "kubernetes_service_v1" "mssql_service" {
  metadata {
    name      = "mssql-svc"
    namespace = local.k8s_shared_namespace
    labels = {
      app = "mssql"
    }
  }
  spec {
    port {
      port        = 1433
      target_port = 1433
    }
    selector = {
      app = "mssql"
    }
    type = "LoadBalancer"
  }
}

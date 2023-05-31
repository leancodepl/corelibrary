resource "kubernetes_deployment_v1" "seq_deployment" {
  metadata {
    name      = "seq"
    namespace = local.k8s_shared_namespace
    labels = {
      app = "seq"
    }
  }
  spec {
    replicas = 1
    selector {
      match_labels = {
        app = "seq"
      }
    }
    template {
      metadata {
        labels = {
          app = "seq"
        }
      }
      spec {
        container {
          env {
            name  = "ACCEPT_EULA"
            value = "Y"
          }
          image = "datalust/seq"
          name  = "seq"
          port {
            container_port = "80"
          }
          port {
            container_port = "5341"
          }
          volume_mount {
            mount_path = "/data"
            name       = "data"
          }
        }
        volume {
          name = "data"
          persistent_volume_claim {
            claim_name = kubernetes_manifest.seq_pvc.manifest.metadata.name
          }
        }
      }
    }
  }
}

resource "kubernetes_manifest" "seq_pvc" {
  manifest = {
    "apiVersion" = "v1"
    "kind"       = "PersistentVolumeClaim"
    "metadata" = {
      "name"      = "seq-pvc"
      "namespace" = local.k8s_shared_namespace,
      "labels" = {
        "app" = "seq"
      }
    }
    "spec" = {
      "accessModes" = [
        "ReadWriteOnce",
      ]
      "resources" = {
        "requests" = {
          "storage" = "1Gi"
        }
      }
    }
  }
}

resource "kubernetes_service_v1" "seq_service" {
  metadata {
    name      = "seq-svc"
    namespace = local.k8s_shared_namespace
    labels = {
      app = "seq"
    }
  }
  spec {
    selector = {
      app = "seq"
    }
    port {
      port        = 80
      target_port = 80
    }
  }
}

resource "kubernetes_ingress_v1" "seq_ingress" {
  metadata {
    name      = "seq-ingress"
    namespace = local.k8s_shared_namespace
    labels = {
      app = "seq"
    }
  }
  spec {
    rule {
      host = "seq.local.lncd.pl"
      http {
        path {
          backend {
            service {
              name = kubernetes_service_v1.seq_service.metadata[0].name
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

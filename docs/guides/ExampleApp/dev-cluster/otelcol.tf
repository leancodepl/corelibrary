locals {
  otelcol_image = "${local.registry_address}/otelcol"
}


resource "kubernetes_secret" "otel_config" {
  metadata {
    name      = "otel-config"
    namespace = local.k8s_shared_namespace
    labels = {
      "app" = "otlp"
    }
  }

  data = {
    "otel-agent-config.yaml" = yamlencode({
      exporters = {
        jaeger = {
          endpoint = "jaeger.shared.svc.cluster.local:14250"
          tls = {
            insecure = true
          }
        }
      }

      extensions = {
        health_check = {}
      }
      processors = {
        batch = {
          timeout = "10s"
        }
        k8sattributes = {
          passthrough = true
        }
        resourcedetection = {
          detectors = [
            "env",
          ]
          override = false
          timeout  = "5s"
        }
      }
      receivers = {
        otlp = {
          protocols = {
            grpc = {
              endpoint = "0.0.0.0:55680"
            }
            http = {
              endpoint = "0.0.0.0:55681"
            }
          }
        }
        jaeger = {
          protocols = {
            thrift_http = {
              endpoint = "0.0.0.0:14268"
            }
          }
        }
      }
      service = {
        extensions = ["health_check"]
        pipelines = {
          traces = {
            exporters = ["jaeger"]
            processors = [
              "batch",
              "resourcedetection",
              "k8sattributes",
            ]
            receivers = ["otlp", "jaeger"]
          }
        }
      }
    })
  }
}

resource "kubernetes_daemonset" "otel_agent" {
  metadata {
    name      = "otel-agent"
    namespace = local.k8s_shared_namespace
    labels = {
      "app" = "otlp"
    }
  }

  spec {
    selector {
      match_labels = {
        app       = "opentelemetry-collector"
        component = "agent"
      }
    }

    template {
      metadata {
        labels = {
          app       = "opentelemetry-collector"
          component = "agent"
        }
      }

      spec {
        volume {
          name = "config"
          secret {
            secret_name = kubernetes_secret.otel_config.metadata[0].name
          }
        }

        container {
          name              = "agent"
          image             = "otel/opentelemetry-collector-contrib:0.51.0"
          image_pull_policy = "Always"

          args = ["--config", "/conf/otel-agent-config.yaml"]

          port {
            host_port      = 55680
            container_port = 55680
          }

          port {
            host_port      = 55681
            container_port = 55681
          }

          port {
            host_port      = 14268
            container_port = 14268
          }

          volume_mount {
            name       = "config"
            mount_path = "/conf"
          }

          liveness_probe {
            http_get {
              path = "/"
              port = "13133"
            }
          }

          readiness_probe {
            http_get {
              path = "/"
              port = "13133"
            }
          }
        }
      }
    }
  }
}

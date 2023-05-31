resource "kubernetes_deployment_v1" "jaeger_deployment" {
  metadata {
    name      = "jaeger"
    namespace = local.k8s_shared_namespace
    labels = {
      app = "jaeger"
    }
  }
  spec {
    replicas = 1
    selector {
      match_labels = {
        app = "jaeger"
      }
    }
    template {
      metadata {
        labels = {
          app = "jaeger"
        }
      }
      spec {
        container {
          image = "jaegertracing/all-in-one"
          name  = "jaeger"
          port {
            name           = "collector-http"
            container_port = 14268
          }
          port {
            name           = "collector-grpc"
            container_port = 14250
          }
          port {
            name           = "frontend"
            container_port = 16686
          }
        }
      }
    }
  }
}

resource "kubernetes_service_v1" "jaeger_service" {
  metadata {
    name      = "jaeger-svc"
    namespace = local.k8s_shared_namespace
    labels = {
      app = "jaeger"
    }
  }
  spec {
    selector = {
      app = "jaeger"
    }
    port {
      name        = "collector-http"
      port        = 14268
      target_port = 14268
    }
    port {
      name        = "collector-grpc"
      port        = 14250
      target_port = 14250
    }
    port {
      name        = "frontend"
      port        = 16686
      target_port = 16686
    }
  }
}

resource "kubernetes_ingress_v1" "jaeger_ingress" {
  metadata {
    name      = "jaeger-ingress"
    namespace = local.k8s_shared_namespace
    labels = {
      app = "jaeger"
    }
  }
  spec {
    rule {
      host = "jaeger.local.lncd.pl"
      http {
        path {
          backend {
            service {
              name = kubernetes_service_v1.jaeger_service.metadata[0].name
              port {
                number = 16686
              }
            }
          }
          path_type = "ImplementationSpecific"
        }
      }
    }
  }
}

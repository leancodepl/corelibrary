locals {
  traefik_image_name    = "${local.registry_address}/traefik"
  traefik_image_version = "2.6.0"
  traefik_image         = "${local.traefik_image_name}:${local.traefik_image_version}"
}


resource "docker_image" "traefik" {
  name = local.traefik_image

  build {
    path       = "./apps"
    dockerfile = "Dockerfile.traefik"
    build_arg = {
      dockerfile_trigger   = filemd5("./apps/Dockerfile.traefik")
      dynamic_toml_trigger = filemd5("./apps/dynamic.toml")
    }
  }

  # Docker provider cannot push to insecure registries
  provisioner "local-exec" {
    command = "docker push ${local.traefik_image}"
  }

  depends_on = [
    null_resource.cluster_kubeconfig
  ]
}

resource "helm_release" "traefik" {
  chart      = "traefik/traefik"
  repository = "helm.traefik.io/traefik"
  version    = "10.13.0"

  name      = "traefik"
  namespace = local.k8s_shared_namespace

  set {
    name  = "image.name"
    value = local.traefik_image_name
  }

  set {
    name  = "image.tag"
    value = local.traefik_image_version
  }

  set {
    name  = "image.pullPolicy"
    value = "Always"
  }

  set {
    name  = "providers.kubernetesIngress.publishedService.enabled"
    value = true
  }

  set {
    name  = "ports.web.redirectTo"
    value = "websecure"
  }
  set {

    name  = "ports.websecure.tls.enabled"
    value = true
  }

  values = [yamlencode({
    globalArguments = [
      "--global.checkNewVersion=true",
      "--global.sendAnonymousUsage=false",
    ],

    additionalArguments = [
      "--providers.file.directory=/config/dynamic",
      "--log.level=DEBUG",
      "--api.insecure=true",
      "--api.debug=true",
      "--accesslog=true",
    ],
  })]

  depends_on = [
    docker_image.traefik
  ]
}

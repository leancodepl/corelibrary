locals {
  registry_name    = "k3d-exampleapp-registry.local.lncd.pl"
  registry_port    = 21345
  registry_address = "${local.registry_name}:${local.registry_port}"
}

resource "null_resource" "cluster" {
  provisioner "local-exec" {
    command = "k3d cluster create -c ${path.module}/k3d.yaml"
  }
}

resource "null_resource" "cluster_kubeconfig" {
  provisioner "local-exec" {
    command = "k3d kubeconfig get exampleapp 2>&1 > .terraform/local_kubeconfig"
  }

  depends_on = [
    null_resource.cluster
  ]
}

data "local_file" "kubeconfig" {
  filename = ".terraform/local_kubeconfig"

  depends_on = [
    null_resource.cluster_kubeconfig
  ]
}

locals {
  kubeconfig = yamldecode(data.local_file.kubeconfig.content)

  credentials = {
    host                   = local.kubeconfig.clusters[0].cluster.server
    cluster_ca_certificate = base64decode(local.kubeconfig.clusters[0].cluster["certificate-authority-data"])
    client_key             = base64decode(local.kubeconfig.users[0].user["client-key-data"])
    client_certificate     = base64decode(local.kubeconfig.users[0].user["client-certificate-data"])
  }
}

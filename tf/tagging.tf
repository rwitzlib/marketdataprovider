locals {
  default_tags = {
    Environment = var.environment
    Service     = var.service_name
    Repo        = "https://github.com/rwitzlib/stockmountain-marketviewer"
  }
}
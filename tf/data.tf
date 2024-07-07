data "aws_ecr_image" "marketviewer" {
  repository_name = "${var.team}-${var.environment}-${var.service_name}"
  image_tag       = var.image_tag
}

data "aws_ssm_parameter" "polygon_token" {
  name = "/tokens/polygon"
}
output "auth_type" {
  value = local.service_auth_type
}

output "auth_context" {
  value = local.service_auth_context
}

output "jobs_url" {
  value = "${local.service_url}/jobs"
}

output "service_definition" {
  value = {
    name      = var.name
    auth_type = local.service_auth_type
    resources = concat([
      {
        resource_type = "AmeJob"
        http_endpoint = "${local.service_url}/jobs"
      },
      {
        resource_type = "AIJob"
        http_endpoint = "${local.service_url}/jobs"
      },
      {
        resource_type = "CaptureJob"
        http_endpoint = "${local.service_url}/jobs"
      },
      {
        resource_type = "DistributionJob"
        http_endpoint = "${local.service_url}/jobs"
      },
      {
        resource_type = "QAJob"
        http_endpoint = "${local.service_url}/jobs"
      },
      {
        resource_type = "TransferJob"
        http_endpoint = "${local.service_url}/jobs"
      },
      {
        resource_type = "TransformJob"
        http_endpoint = "${local.service_url}/jobs"
      },
      {
        resource_type = "WorkflowJob"
        http_endpoint = "${local.service_url}/jobs"
      }
      ], [for job_type in var.custom_job_types : {
        resource_type = job_type
        http_endpoint = "${local.service_url}/jobs"
    }]),
    job_profiles = []
  }
}
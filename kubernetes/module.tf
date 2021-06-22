terraform {
  required_providers {
    helm = {
      source = "hashicorp/helm"
      version = ">= 2.2.0"
    }
  }
}

provider "helm" {
  kubernetes {
    config_path = var.kubeconfig_path
  }
}

locals {
  values = [yamlencode({
    namespace = var.kubernetes_namespace
    serviceName = var.service_name
    servicesUrl = var.services_url
    mongoDb = {
      connectionString = var.mongodb_connection_string,
      databaseName = var.mongodb_database_name
      collectionName = var.mongodb_collection_name
    }
    kafka = {
      bootstrapServers = var.kafka_bootstrap_servers
      workerTopic = var.kafka_worker_topic
      cronJobStateTopic = var.kafka_cron_job_state_topic
      cronJobExecutionTopicPrefix = var.kafka_cron_job_execution_topic_prefix
    }
    apiHandler = {
      dockerImageId = var.api_handler_docker_image_id
      numReplicas = var.api_handler_num_replicas
    }
    worker = {
      dockerImageId = var.worker_docker_image_id
      numReplicas = var.worker_num_replicas
    }
    cronJobs = {
      dockerImageId = var.cron_jobs_docker_image_id
      numReplicas = var.cron_jobs_num_replicas
    }
    jobChecker = {
      dockerImageId = var.job_checker_docker_image_id
      numReplicas = var.job_checker_num_replicas
    }
    jobCleanup = {
      dockerImageId = var.job_cleanup_docker_image_id
      numReplicas = var.job_cleanup_num_replicas
    }
    defaultJobTimeoutInMinutes = var.default_job_timeout_in_minutes
    c = var.job_retention_period_in_days
  })]
}

resource "helm_release" "release" {
  name      = var.service_name
  chart     = "${path.module}/helm/job-processor"
  values    = local.values
  namespace = var.kubernetes_namespace
}
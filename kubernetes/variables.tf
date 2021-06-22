variable "service_name" {
    default = "job-processor"
}
variable "services_url" {
    default = "http://service-registry/mcma/api/services"
}

variable "kubeconfig_path" {
    default = "~/.kube/config"
}
variable "kubernetes_namespace" {
    default = "default"
}

variable "mongodb_connection_string" {
    type = string
}
variable "mongodb_database_name" {
    default = "mcma"
}
variable "mongodb_collection_name" {
    default = "job-processor"
}

variable "kafka_bootstrap_servers" {
    type = string
}
variable "kafka_worker_topic" {
    default = "mcma.jobprocessor.worker"
}
variable "kafka_cron_job_state_topic" {
    default = "mcma.jobprocessor.cronjobs.state"
}
variable "kafka_cron_job_execution_topic_prefix" {
    default = "mcma.jobprocessor.cronjobs.execution"
}

variable "api_handler_docker_image_id" {
    default = "evanverneyfink/mcma-job-processor-api"
}
variable "api_handler_num_replicas" { 
    default = 1
}

variable "worker_docker_image_id" {
    default = "evanverneyfink/mcma-job-processor-worker"
}
variable "worker_num_replicas" {
    default = 1
}

variable "cron_jobs_docker_image_id" {
    default = "evanverneyfink/mcma-job-processor-cron-jobs"
}
variable "cron_jobs_num_replicas" {
    default = 1
}

variable "job_checker_docker_image_id" {
    default = "evanverneyfink/mcma-job-processor-job-checker"
}
variable "job_checker_num_replicas" {
    default = 1
}

variable "job_cleanup_docker_image_id" {
    default = "evanverneyfink/mcma-job-processor-job-cleanup"
}
variable "job_cleanup_num_replicas" {
    default = 1
}

variable "default_job_timeout_in_minutes" {
    type        = number
    description = "Set default job timeout in minutes"
    default     = 60
}
variable "job_retention_period_in_days" {
    type        = number
    description = "Set job retention period in days"
    default     = 90
}
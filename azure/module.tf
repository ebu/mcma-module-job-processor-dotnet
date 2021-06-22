terraform {
  required_version = "~> 0.15.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "2.60.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "1.5.0"
    }
  }
}

provider "azurerm" {
  subscription_id = var.azure_subscription_id
  features {}
}

provider "azuread" {
  subscription_id = var.azure_subscription_id
  tenant_id       = var.azure_tenant_id
}

locals {
  job_processor_worker_zip_file = "./../services/JobProcessor/Mcma.Azure.JobProcessor.Worker/dist/function.zip"

  job_processor_api_zip_file      = "./../services/JobProcessor/Mcma.Azure.JobProcessor.ApiHandler/dist/function.zip"
  job_processor_api_function_name = "${var.prefix}-job-processor-api"
  job_processor_api_url           = "https://${local.job_processor_api_function_name}.azurewebsites.net"

  job_processor_job_checker_zip_file      = "./../services/JobProcessor/Mcma.Azure.JobProcessor.PeriodicJobChecker/dist/function.zip"
  job_processor_job_checker_function_name = "${var.prefix}-job-processor-periodic-job-checker"
  job_processor_job_checker_url           = "https://${local.job_processor_job_checker_function_name}.azurewebsites.net"
  job_processor_job_checker_workflow_name = "CheckForStalledJobs"

  job_processor_job_cleanup_zip_file      = "./../services/JobProcessor/Mcma.Azure.JobProcessor.PeriodicJobCleanup/dist/function.zip"
  job_processor_job_cleanup_function_name = "${var.prefix}-job-processor-periodic-job-cleanup"
  job_processor_job_cleanup_url           = "https://${local.job_processor_job_cleanup_function_name}.azurewebsites.net"

  service_url          = local.job_processor_api_url
  service_auth_type    = "AzureAD"
  service_auth_context = "{ \"scope\": \"${local.service_url}/.default\" }"
}

data "azurerm_subscription" "primary" {}

data "azurerm_resource_group" "resource_group" {
  name = var.azure_resource_group_name
}

data "azurerm_app_service_plan" "app_service_plan" {
  resource_group_name = data.azurerm_resource_group.resource_group.name
  name                = var.azure_app_service_plan_name
}

data "azurerm_cosmosdb_account" "cosmos_db_account" {
  resource_group_name = data.azurerm_resource_group.resource_group.name
  name                = var.cosmosdb_account_name
}

data "azurerm_application_insights" "appinsights" {
  resource_group_name = data.azurerm_resource_group.resource_group.name
  name                = var.appinsights_name
}

data "azurerm_storage_account" "app_storage_account" {
  resource_group_name = data.azurerm_resource_group.resource_group.name
  name                = var.app_storage_account_name
}

data "azurerm_storage_container" "app_storage_deploy_container" {
  resource_group_name  = data.azurerm_resource_group.resource_group.name
  storage_account_name = data.azurerm_storage_account.app_storage_account.name
  name                 = var.app_storage_deploy_container
}

data "azurerm_storage_account_sas" "app_storage_sas" {
  connection_string = data.azurerm_storage_account.app_storage_account.primary_connection_string
  https_only        = true

  resource_types {
    service   = false
    container = false
    object    = true
  }

  services {
    blob  = true
    queue = false
    table = false
    file  = false
  }

  start  = "2020-09-08"
  expiry = "2021-09-08"

  permissions {
    read    = true
    write   = false
    delete  = false
    list    = false
    add     = false
    create  = false
    update  = false
    process = false
  }
}

resource "azurerm_cosmosdb_sql_database" "cosmosdb_database" {
  name                = "ServiceRegistry"
  resource_group_name = data.azurerm_resource_group.resource_group.name
  account_name        = data.azurerm_cosmosdb_account.cosmos_db_account.name
}

resource "azurerm_cosmosdb_sql_container" "cosmosdb_container" {
  name                = "ServiceRegistry"
  resource_group_name = data.azurerm_resource_group.resource_group.name
  account_name        = data.azurerm_cosmosdb_account.cosmos_db_account.name
  database_name       = azurerm_cosmosdb_sql_database.cosmosdb_database.id
  partition_key_path  = "/partitionKey"
}

resource "azurerm_role_definition" "job_checker_workflow_toggler_role" {
  name  = "${var.prefix}-job-checker-workflow-toggler"
  scope = data.azurerm_subscription.primary.id

  permissions {
    actions = [
      "Microsoft.Logic/workflows/enable/action",
      "Microsoft.Logic/workflows/disable/action",
    ]
    not_actions = []
  }

  assignable_scopes = [
  data.azurerm_subscription.primary.id]
}

resource "azurerm_cosmosdb_sql_container" "job_processor_cosmosdb_container" {
  name                = "JobProcessor"
  resource_group_name = data.azurerm_resource_group.resource_group.name
  account_name        = data.azurerm_cosmosdb_account.cosmos_db_account.name
  database_name       = var.cosmosdb_database_id
  partition_key_path  = "/partitionKey"
}

#===================================================================
# Worker Function
#===================================================================

resource "azurerm_storage_queue" "worker_function_queue" {
  name                 = "job-processor-work-queue"
  storage_account_name = data.azurerm_storage_account.app_storage_account.name
}

resource "azurerm_storage_blob" "worker_function_zip" {
  name                   = "job-processor/worker/function_${filesha256(local.job_processor_worker_zip_file)}.zip"
  storage_account_name   = data.azurerm_storage_account.app_storage_account.name
  storage_container_name = data.azurerm_storage_container.app_storage_deploy_container.name
  type                   = "Block"
  source                 = local.job_processor_worker_zip_file
}

resource "azurerm_function_app" "worker_function" {
  name                       = "${var.prefix}-job-processor-worker"
  location                   = var.azure_location
  resource_group_name        = data.azurerm_resource_group.resource_group.name
  app_service_plan_id        = data.azurerm_app_service_plan.app_service_plan.id
  storage_account_name       = data.azurerm_storage_account.app_storage_account.name
  storage_account_access_key = data.azurerm_storage_account_sas.app_storage_sas.sas
  version                    = "~3"

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME       = "dotnet"
    FUNCTION_APP_EDIT_MODE         = "readonly"
    https_only                     = true
    HASH                           = filesha256(local.job_processor_worker_zip_file)
    WEBSITE_RUN_FROM_PACKAGE       = "${data.azurerm_storage_container.app_storage_deploy_container.id}/${azurerm_storage_blob.worker_function_zip.name}${data.azurerm_storage_account_sas.app_storage_sas.sas}"
    APPINSIGHTS_INSTRUMENTATIONKEY = data.azurerm_application_insights.appinsights.instrumentation_key

    MCMA_WORK_QUEUE_STORAGE        = data.azurerm_storage_account.app_storage_account.primary_connection_string
    MCMA_TABLE_NAME                = azurerm_cosmosdb_sql_container.job_processor_cosmosdb_container.name
    MCMA_PUBLIC_URL                = local.job_processor_api_url
    MCMA_AZURE_SUBSCRIPTION_ID     = data.azurerm_subscription.primary.id
    MCMA_AZURE_TENANT_ID           = var.azure_tenant_id
    MCMA_AZURE_RESOURCE_GROUP_NAME = data.azurerm_resource_group.resource_group.name
    MCMA_COSMOS_DB_ENDPOINT        = data.azurerm_cosmosdb_account.cosmos_db_account.endpoint
    MCMA_COSMOS_DB_KEY             = data.azurerm_cosmosdb_account.cosmos_db_account.primary_master_key
    MCMA_COSMOS_DB_DATABASE_ID     = azurerm_cosmosdb_sql_database.cosmosdb_database.id
    MCMA_COSMOS_DB_REGION          = var.azure_location
    MCMA_SERVICES_URL              = var.service_registry.services_url
    MCMA_SERVICES_AUTH_TYPE        = var.service_registry.auth_type
    MCMA_SERVICES_AUTH_CONTEXT     = var.service_registry.auth_context
    MCMA_JOB_CHECKER_WORKFLOW_NAME = local.job_processor_job_checker_workflow_name
  }

  provisioner "local-exec" {
    command = "az webapp start --resource-group ${data.azurerm_resource_group.resource_group.name} --name ${azurerm_function_app.worker_function.name}"
  }
}

resource "azurerm_role_assignment" "job_processor_worker_role_assignment" {
  scope              = data.azurerm_subscription.primary.id
  role_definition_id = azurerm_role_definition.job_checker_workflow_toggler_role.id
  principal_id       = azurerm_function_app.worker_function.identity[0].principal_id
}

#===================================================================
# API Function
#===================================================================

resource "azuread_application" "job_processor_api_app" {
  name = local.job_processor_api_function_name
  identifier_uris = [
  local.job_processor_api_url]
}

resource "azuread_service_principal" "job_processor_api_sp" {
  application_id               = azuread_application.job_processor_api_app.application_id
  app_role_assignment_required = false
}

resource "azurerm_storage_blob" "job_processor_api_function_zip" {
  name                   = "job-processor/api/function_${filesha256(local.job_processor_api_zip_file)}.zip"
  storage_account_name   = data.azurerm_storage_account.app_storage_account.name
  storage_container_name = data.azurerm_storage_container.app_storage_deploy_container.name
  type                   = "Block"
  source                 = local.job_processor_api_zip_file
}

resource "azurerm_function_app" "job_processor_api_function" {
  name                       = local.job_processor_api_function_name
  location                   = var.azure_location
  resource_group_name        = data.azurerm_resource_group.resource_group.name
  app_service_plan_id        = data.azurerm_app_service_plan.app_service_plan.id
  storage_account_name       = data.azurerm_storage_account.app_storage_account.name
  storage_account_access_key = data.azurerm_storage_account.app_storage_account.primary_access_key
  version                    = "~3"

  identity {
    type = "SystemAssigned"
  }

  auth_settings {
    enabled                       = true
    issuer                        = "https://sts.windows.net/${var.azure_tenant_id}"
    default_provider              = "AzureActiveDirectory"
    unauthenticated_client_action = "RedirectToLoginPage"
    active_directory {
      client_id = azuread_application.job_processor_api_app.application_id
      allowed_audiences = [
        local.job_processor_api_url
      ]
    }
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME       = "dotnet"
    FUNCTION_APP_EDIT_MODE         = "readonly"
    https_only                     = true
    HASH                           = filesha256(local.job_processor_api_zip_file)
    WEBSITE_RUN_FROM_PACKAGE       = "${data.azurerm_storage_container.app_storage_deploy_container.id}/${azurerm_storage_blob.job_processor_api_function_zip.name}${data.azurerm_storage_account_sas.app_storage_sas.sas}"
    APPINSIGHTS_INSTRUMENTATIONKEY = data.azurerm_application_insights.appinsights.instrumentation_key

    MCMA_TABLE_NAME                = azurerm_cosmosdb_sql_container.job_processor_cosmosdb_container.name
    MCMA_PUBLIC_URL                = local.job_processor_api_url
    MCMA_AZURE_SUBSCRIPTION_ID     = data.azurerm_subscription.primary.id
    MCMA_AZURE_TENANT_ID           = var.azure_tenant_id
    MCMA_AZURE_RESOURCE_GROUP_NAME = data.azurerm_resource_group.resource_group.name
    MCMA_COSMOS_DB_ENDPOINT        = data.azurerm_cosmosdb_account.cosmos_db_account.endpoint
    MCMA_COSMOS_DB_KEY             = data.azurerm_cosmosdb_account.cosmos_db_account.primary_master_key
    MCMA_COSMOS_DB_DATABASE_ID     = azurerm_cosmosdb_sql_database.cosmosdb_database.id
    MCMA_COSMOS_DB_REGION          = var.azure_location
    MCMA_SERVICES_URL              = var.service_registry.services_url
    MCMA_SERVICES_AUTH_TYPE        = var.service_registry.auth_type
    MCMA_SERVICES_AUTH_CONTEXT     = var.service_registry.auth_context
    MCMA_WORKER_QUEUE_NAME         = azurerm_storage_queue.worker_function_queue.name
  }
}

#===================================================================
# Job Checker Function
#===================================================================

resource "azuread_application" "job_processor_job_checker_app" {
  name = local.job_processor_job_checker_function_name
  identifier_uris = [
    local.job_processor_job_checker_url
  ]
}

resource "azuread_service_principal" "job_processor_job_checker_sp" {
  application_id               = azuread_application.job_processor_job_checker_app.application_id
  app_role_assignment_required = false
}

resource "azurerm_storage_blob" "job_processor_job_checker_function_zip" {
  name                   = "job-processor/job-checker/function_${filesha256(local.job_processor_job_checker_zip_file)}.zip"
  storage_account_name   = data.azurerm_storage_account.app_storage_account.name
  storage_container_name = data.azurerm_storage_container.app_storage_deploy_container.name
  type                   = "Block"
  source                 = local.job_processor_job_checker_zip_file
}

resource "azurerm_function_app" "job_processor_job_checker_function" {
  name                       = local.job_processor_job_checker_function_name
  location                   = var.azure_location
  resource_group_name        = data.azurerm_resource_group.resource_group.name
  app_service_plan_id        = data.azurerm_app_service_plan.app_service_plan.id
  storage_account_name       = data.azurerm_storage_account.app_storage_account.name
  storage_account_access_key = data.azurerm_storage_account.app_storage_account.primary_access_key
  version                    = "~3"

  identity {
    type = "SystemAssigned"
  }

  auth_settings {
    enabled                       = true
    issuer                        = "https://sts.windows.net/${var.azure_tenant_id}"
    default_provider              = "AzureActiveDirectory"
    unauthenticated_client_action = "RedirectToLoginPage"
    active_directory {
      client_id = azuread_application.job_processor_job_checker_app.application_id
      allowed_audiences = [
      local.job_processor_job_checker_url]
    }
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME       = "dotnet"
    FUNCTION_APP_EDIT_MODE         = "readonly"
    https_only                     = true
    HASH                           = filesha256(local.job_processor_job_checker_zip_file)
    WEBSITE_RUN_FROM_PACKAGE       = "${data.azurerm_storage_container.app_storage_deploy_container.id}/${azurerm_storage_blob.job_processor_job_checker_function_zip.name}${data.azurerm_storage_account_sas.app_storage_sas.sas}"
    APPINSIGHTS_INSTRUMENTATIONKEY = data.azurerm_application_insights.appinsights.instrumentation_key

    MCMA_TABLE_NAME                = azurerm_cosmosdb_sql_container.job_processor_cosmosdb_container.name
    MCMA_PUBLIC_URL                = local.job_processor_api_url
    MCMA_AZURE_SUBSCRIPTION_ID     = var.azure_subscription_id
    MCMA_AZURE_TENANT_ID           = var.azure_tenant_id
    MCMA_AZURE_RESOURCE_GROUP_NAME = data.azurerm_resource_group.resource_group.name
    MCMA_COSMOS_DB_ENDPOINT        = data.azurerm_cosmosdb_account.cosmos_db_account.endpoint
    MCMA_COSMOS_DB_KEY             = data.azurerm_cosmosdb_account.cosmos_db_account.primary_master_key
    MCMA_COSMOS_DB_DATABASE_ID     = azurerm_cosmosdb_sql_database.cosmosdb_database.id
    MCMA_COSMOS_DB_REGION          = var.azure_location
    MCMA_WORKER_QUEUE_NAME         = azurerm_storage_queue.worker_function_queue.name
    MCMA_JOB_CHECKER_WORKFLOW_NAME = local.job_processor_job_checker_workflow_name
  }
}

resource "azurerm_template_deployment" "job_processor_job_checker_workflow" {
  name                = local.job_processor_job_checker_workflow_name
  resource_group_name = data.azurerm_resource_group.resource_group.name
  deployment_mode     = "Incremental"

  parameters = {
    workflowName        = local.job_processor_job_checker_workflow_name
    functionUrl         = local.job_processor_job_checker_url
    recurrenceFrequency = "Minute"
    recurrenceInterval  = 2
    state               = "Disabled"
  }

  template_body = <<DEPLOY
{
    "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {
        "workflowName": { "type": "string" },
        "functionUrl": { "type": "string" },
        "recurrenceFrequency": { "type": "string" },
        "recurrenceInterval": { "type": "string" },
        "state": { "type": "string" }
    },
    "variables": {},
    "resources": [
        {
            "type": "Microsoft.Logic/workflows",
            "apiVersion": "2017-07-01",
            "name": "[parameters('workflowName')]",
            "location": "[resourceGroup().location]",
            "identity": {
               "type": "SystemAssigned"
            },
            "properties": {
                "state": "[parameters('state')]",
                "definition": {
                    "$schema": "https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#",
                    "contentVersion": "1.0.0.0",
                    "parameters": {},
                    "triggers": {
                        "Recurrence": {
                            "recurrence": {
                                "frequency": "[parameters('recurrenceFrequency')]",
                                "interval": "[parameters('recurrenceInterval')]"
                            },
                            "type": "Recurrence"
                        }
                    },
                    "actions": {
                        "HTTP": {
                            "runAfter": {},
                            "type": "Http",
                            "inputs": {
                                "authentication": {
                                    "type": "ManagedServiceIdentity",
                                    "audience": "[parameters('functionUrl')]"
                                },
                                "method": "POST",
                                "uri": "[parameters('functionUrl')]"
                            }
                        }
                    },
                    "outputs": {}
                },
                "parameters": {}
            }
        }
    ]
}
DEPLOY
}

resource "azurerm_role_assignment" "job_processor_job_checker_role_assignment" {
  scope              = data.azurerm_subscription.primary.id
  role_definition_id = azurerm_role_definition.job_checker_workflow_toggler_role.id
  principal_id       = azurerm_function_app.job_processor_job_checker_function.identity[0].principal_id
}


#===================================================================
# Job Cleanup Function
#===================================================================

resource "azuread_application" "job_processor_job_cleanup_app" {
  name = local.job_processor_job_cleanup_function_name
  identifier_uris = [
  local.job_processor_job_cleanup_url]
}

resource "azuread_service_principal" "job_processor_job_cleanup_sp" {
  application_id               = azuread_application.job_processor_job_cleanup_app.application_id
  app_role_assignment_required = false
}

resource "azurerm_storage_blob" "job_processor_job_cleanup_function_zip" {
  name                   = "job-processor/job-cleanup/function_${filesha256(local.job_processor_job_cleanup_zip_file)}.zip"
  storage_account_name   = data.azurerm_storage_account.app_storage_account.name
  storage_container_name = data.azurerm_storage_container.app_storage_deploy_container.name
  type                   = "Block"
  source                 = local.job_processor_job_cleanup_zip_file
}

resource "azurerm_function_app" "job_processor_job_cleanup_function" {
  name                       = local.job_processor_job_cleanup_function_name
  location                   = var.azure_location
  resource_group_name        = data.azurerm_resource_group.resource_group.name
  app_service_plan_id        = data.azurerm_app_service_plan.app_service_plan.id
  storage_account_name       = data.azurerm_storage_account.app_storage_account.name
  storage_account_access_key = data.azurerm_storage_account.app_storage_account.primary_access_key
  version                    = "~3"

  auth_settings {
    enabled                       = true
    issuer                        = "https://sts.windows.net/${var.azure_tenant_id}"
    default_provider              = "AzureActiveDirectory"
    unauthenticated_client_action = "RedirectToLoginPage"
    active_directory {
      client_id = azuread_application.job_processor_job_cleanup_app.application_id
      allowed_audiences = [
      local.job_processor_job_cleanup_url]
    }
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME       = "dotnet"
    FUNCTION_APP_EDIT_MODE         = "readonly"
    https_only                     = true
    HASH                           = filesha256(local.job_processor_job_cleanup_zip_file)
    WEBSITE_RUN_FROM_PACKAGE       = "${data.azurerm_storage_container.app_storage_deploy_container.id}/${azurerm_storage_blob.job_processor_job_cleanup_function_zip.name}${data.azurerm_storage_account_sas.app_storage_sas.sas}"
    APPINSIGHTS_INSTRUMENTATIONKEY = data.azurerm_application_insights.appinsights.instrumentation_key

    MCMA_TABLE_NAME                = azurerm_cosmosdb_sql_container.job_processor_cosmosdb_container.name
    MCMA_PUBLIC_URL                = local.job_processor_api_url
    MCMA_AZURE_SUBSCRIPTION_ID     = var.azure_subscription_id
    MCMA_AZURE_TENANT_ID           = var.azure_tenant_id
    MCMA_AZURE_RESOURCE_GROUP_NAME = data.azurerm_resource_group.resource_group.name
    MCMA_COSMOS_DB_ENDPOINT        = data.azurerm_cosmosdb_account.cosmos_db_account.endpoint
    MCMA_COSMOS_DB_KEY             = data.azurerm_cosmosdb_account.cosmos_db_account.primary_master_key
    MCMA_COSMOS_DB_DATABASE_ID     = azurerm_cosmosdb_sql_database.cosmosdb_database.id
    MCMA_COSMOS_DB_REGION          = var.azure_location
    MCMA_WORKER_QUEUE_NAME         = azurerm_storage_queue.worker_function_queue.name
  }
}


resource "azurerm_template_deployment" "job_processor_job_cleanup_workflow" {
  name                = "CleanupOldJobs"
  resource_group_name = data.azurerm_resource_group.resource_group.name
  deployment_mode     = "Incremental"

  parameters = {
    workflowName        = "CleanupOldJobs"
    functionUrl         = local.job_processor_job_cleanup_url
    recurrenceFrequency = "Day"
    recurrenceInterval  = 2
    state               = "Enabled"
  }

  template_body = <<DEPLOY
{
    "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {
        "workflowName": { "type": "string" },
        "functionUrl": { "type": "string" },
        "recurrenceFrequency": { "type": "string" },
        "recurrenceInterval": { "type": "string" },
        "state": { "type": "string" }
    },
    "variables": {},
    "resources": [
        {
            "type": "Microsoft.Logic/workflows",
            "apiVersion": "2017-07-01",
            "name": "[parameters('workflowName')]",
            "location": "[resourceGroup().location]",
            "identity": {
               "type": "SystemAssigned"
            },
            "properties": {
                "state": "[parameters('state')]",
                "definition": {
                    "$schema": "https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#",
                    "contentVersion": "1.0.0.0",
                    "parameters": {},
                    "triggers": {
                        "Recurrence": {
                            "recurrence": {
                                "frequency": "[parameters('recurrenceFrequency')]",
                                "interval": "[parameters('recurrenceInterval')]"
                            },
                            "type": "Recurrence"
                        }
                    },
                    "actions": {
                        "HTTP": {
                            "runAfter": {},
                            "type": "Http",
                            "inputs": {
                                "authentication": {
                                    "type": "ManagedServiceIdentity",
                                    "audience": "[parameters('functionUrl')]"
                                },
                                "method": "POST",
                                "uri": "[parameters('functionUrl')]"
                            }
                        }
                    },
                    "outputs": {}
                },
                "parameters": {}
            }
        }
    ]
}
DEPLOY
}

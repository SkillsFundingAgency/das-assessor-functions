# ![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png) Digital Apprenticeships Service

##  EPAO Onboarding and Certification - Assessor Functions

### Developer Setup

#### Requirements

- Install [.NET Core 3.1](https://www.microsoft.com/net/download)
- Install [Azure Functions SDK](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

### Configuration

1) Create a local.settings.json file (Copy to Output Directory = Copy always) with the following contents:

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "AzureWebJobsDashboard": "UseDevelopmentStorage=true",

        "AppName": "das-assessor-functions",
        "EnvironmentName": "LOCAL",
        "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true",
    }
}
```

1) Ensure that an instance of the SFA.DAS.AssessorService.Application.Api project is running in advance.
	- this may be either locally or remotely however this will expose a database connection

2) The BaseAddress in the configuration table of the local Azure Storage account must be set to the base address of the running 
instance of SFA.DAS.AssessorService.Application.Api project to which the local system should connect.

3) The DC API requires a ClientSecret which can be obtained for a specific test environment from the DC team. 

### Refresh Ilrs

1) When the RefreshIlrsEnqueueProviders function starts and there are any providers updated in the DC API since the RefreshIlrsLastRunDate
in the assessor, a Azure Storage Queue will be created automatically; the name of the queue is defined in [QueueNames.cs](src\SFA.DAS.Assessor.Functions\Infrastructure\QueueNames.cs).

2) The RefreshIlrsEnqueueProviders and RefreshIlrsDequeueProviders functions connect to the DC API, optionally the Configuration can be updated to specify that a Mock should be used for the DC API. The current value of the RefreshIlrsLastRunDate in the Assessor Settings controls the amount of Mock data which is generated. 
      

### Opportunity Finder DataSync

No specific configuration

    




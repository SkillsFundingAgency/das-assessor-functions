# ![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png) Digital Apprenticeships Service

##  EPAO Onboarding and Certification - Assessor Functions

### Developer Setup

#### Requirements

- Install [.NET Core 2.1](https://www.microsoft.com/net/download)
- Install [Azure Functions SDK](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

### Configuration

1) Ensure that an instance of the SFA.DAS.AssessorService.Application.Api project is running in advance.
	- this may be either locally or remotely however this will expose a database connection

2) The BaseAddress in the configuration table of the local Azure Storage account must be set to the base address of the running 
instance of SFA.DAS.AssessorService.Application.Api project to which the local system should connect.

3) The DC API requires a user name and password which can be obtained for a specific test environment from the DC team. 

### Epao DataSync

1) In the local.settings.json update the EpaoServiceBusConnectionString; for local development an azure account is required in which a service bus can
be created, for a getting started guide see https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-portal; the EpaoProviderMessageQueueName
in the local.settings.json file is the name of the service bus queue to create in the azure account.

### Opportunity Finder DataSync

No specific configuration

    




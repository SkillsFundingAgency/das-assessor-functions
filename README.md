# ![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png) Digital Apprenticeships Service

##  EPAO Onboarding and Certification - Assessor Functions

### Developer Setup

#### Requirements

- Install [.NET Core 2.1](https://www.microsoft.com/net/download)
- Install [Azure Functions SDK](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

### Opportunity Finder DataSync

1) Ensure that an instance of the SFA.DAS.AssessorService.Application.Api project is running in advance.
	- this may be either locally or remotely however this will expose a database connection

2) The BaseAddress in the configuration table of the local Azure Storeage account must be set to the base address of the running 
instance of SFA.DAS.AssessorService.Application.Api project to which the local system should connect.

    




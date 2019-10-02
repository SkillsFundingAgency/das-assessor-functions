# ![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png) Digital Apprenticeships Service

##  EPAO Onboarding and Certification - Assessor Functions

[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/Endpoint%20Assessment%20Organisation/das-assessor-functions?branchName=master)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=1805&branchName=master)

### Developer Setup

#### Requirements

- Install [.NET Core 2.1](https://www.microsoft.com/net/download)
- Install [Visual Studio 2019](https://www.visualstudio.com/downloads/) with these workloads:
    - ASP.NET and web development
    - Azure development
- or
- Install the editor of your choice: (caveat emptor)
	- [Jetbrains Rider](https://www.jetbrains.com/rider/)
	- [Visual Studio Code](https://code.visualstudio.com/)
    
- Install [SQL Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- Install [Azure Storage Emulator](https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409) (Make sure you are on v5.3)
- Install [Azure Storage Explorer](http://storageexplorer.com/)

- Administrator Access

#### Setup

- Grab the das-assessor-functions configuration json file from [das-employer-config](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-assessor-functions/SFA.DAS.AssessorFunctions.json)
- Create a Configuration table in your (Development) local Azure Storage account.
- Add a row to the Configuration table with fields: PartitionKey: LOCAL, RowKey: SFA.DAS.AssessorFunctions_1.0, Data: {The contents of the local config json file}.
- Alter the SqlConnectionString value(s) in the json to point to your databases. (required for WorkflowMigrator)

#### Open the solution

- Open Visual studio as an administrator
- Open the solution

## Running the solution

### Opportunity Finder DataSync

1) Ensure that an instance of the SFA.DAS.AssessorService.Application.Api project is running in advance.
	- this may be either locally or remotely however this will expose a database connection

2) The BaseAddress in the configuration table of the local Azure Storeage account must be set to the base address of the running 
instance of SFA.DAS.AssessorService.Application.Api project to which the local system should connect.

### Workflow Migrator
     
(this section to be added later)	 
	 
    




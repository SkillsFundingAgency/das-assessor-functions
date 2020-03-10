# Digital Apprenticeships Service

##  EPAO Onboarding and Certification

|               |               |
| ------------- | ------------- |
|![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png)|Assessor Functions|
| Info | Azure function library containing jobs which support the Assessor service  |
| Build | [![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/Endpoint%20Assessment%20Organisation/das-assessor-functions?branchName=master)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=1805&branchName=master) |
| Func  | Opportunity Finder DataSync - Scheduled rebuild of standard summary |
| Func  | Migrator - Migrates applications to the QnA API (redundant) |

### Developer Setup

#### Requirements

- Install [.NET Core 2.1 SDK](https://www.microsoft.com/net/download)
- Install [Visual Studio 2019](https://www.visualstudio.com/downloads/) with these workloads:
    - Azure development
- Install [SQL Server 2017 Developer Edition](https://go.microsoft.com/fwlink/?linkid=853016)
- Install [SQL Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- Install [Azure Storage Emulator](https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409) (Make sure you are on atleast v5.3)
- Install [Azure Storage Explorer](http://storageexplorer.com/) 
- Administrator Access

##### Alternative Requirements (caveat emptor)

- Install [Visual Studio 2017](https://www.visualstudio.com/downloads/) with these workloads:
  - Azure development
- Install [Azure Functions SDK](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

#### Setup

- Clone this repository
- Open Visual Studio as an administrator

##### Publish Database
The Assessor functions do not not have a database, they accesses the EPAO Assessor service via its internal API.

##### Config

- Get the das-asessor-functions configuration json file from [das-employer-config](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-assessor-functions/SFA.DAS.AssessorFunctions.json); which is a non-public repository.
- Create a Configuration table in your (Development) local Azure Storage account.
- Add a row to the Configuration table with fields: PartitionKey: LOCAL, RowKey: SFA.DAS.AssessorFunctions_1.0, Data: {{The contents of the local config json file}}.

##### Run the solution

- Set SFA.DAS.Assessor.Functions as the startup project
- Running the solution will start all the functions using their default start mechanism (note some may run on startup, others may be scheduled)
- JSON configuration was created to work with dotnet run

-or-

- Navigate to src/SFA.DAS.Assessor.Functions/
- run `dotnet restore`
- run `dotnet run`
- all the functions will start using their default start mechanism (note some may run on startup, others may be scheduled)

Note: Occasionaly subsequent runs may not start correctly, ensure all command prompts have been shut down correctly.

#### To run a local copy you will also require 
To use Opportunity Finder DataSync; you will need to have the SFA.DAS.AssessorService.Application.Api projects running, from the das-assessor-service.

- [Assessor Service](https://github.com/SkillsFundingAgency/das-assessor-service)

##### And you may also require 
To use the migrator (which is currently redundant in live):
- Databases for the Apply, Assessor and QnA Databases in a pre-migration state.
    




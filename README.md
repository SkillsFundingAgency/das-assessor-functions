#  EPAO Onboarding and Certification - Assessor Functions
<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/das-assessor-functions?repoName=SkillsFundingAgency%2Fdas-assessor-functions&branchName=master)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=2539&repoName=SkillsFundingAgency%2Fdas-assessor-functions&branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-assessor-functions&metric=alert_status)](https://sonarcloud.io/project/overview?id=SkillsFundingAgency_das-assessor-functions)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

This repository represents the Assessor Functions code base. This is a service...

# Developer Setup
### Requirements

In order to run this solution locally you will need:
- Install [.NET Core 3.1](https://www.microsoft.com/net/download)
- Install [Azure Functions SDK](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

### Environment Setup

* **local.settings.json** - Create a `local.settings.json` file (Copy to Output Directory = Copy always) with the following data:

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

* **Azure Table Storage Explorer** - Add the following to your Azure Table Storage Explorer.

    Row Key: SFA.DAS.AssessorFunctions_1.0

    Partition Key: LOCAL

    Data: [data](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-assessor-functions/SFA.DAS.AssessorFunctions.json)
    
### Running

* Ensure that an instance of the `SFA.DAS.AssessorService.Application.Api` project is running in advance.
    * This may be either locally or remotely however this will expose a database connection.
* The BaseAddress value in the configuration table of the local Azure Storage account must be set to the base address of the running instance of `SFA.DAS.AssessorService.Application.Api` project (to which the local system should connect).
* The DC API requires a ClientSecret which can be obtained for a specific test environment from the DC team. 

#### Refresh Ilrs

* When the `RefreshIlrsEnqueueProviders` function starts and there are providers updated in the DC API since the `RefreshIlrsLastRunDate` function in the assessor, an Azure Storage Queue will be created automatically. The name of the queue is defined in [QueueNames.cs](src\SFA.DAS.Assessor.Functions\Infrastructure\QueueNames.cs).
* The `RefreshIlrsEnqueueProviders` and `RefreshIlrsDequeueProviders` functions connect to the DC API, optionally the Configuration can be updated to specify that a Mock should be used for the DC API. The current value of the `RefreshIlrsLastRunDate` in the Assessor Settings controls the amount of Mock data which is generated. 
      
#### Opportunity Finder DataSync

No specific configuration

### Testing

This codebase includes unit tests and integration tests. These are all in seperate projects aptly named after the project that the tests cover.

#### Unit Tests

There are several unit test projects in the solution built using C#, .NET , FluentAssertions, Moq, NUnit, and AutoFixture.
* `SFA.DAS.Assessor.Functions.ExternalApis.UnitTests`
* `SFA.DAS.Assessor.Functions.UnitTests`

#### Integration Tests

There is one integration test project in the solution, `SFA.DAS.Assessor.Functions.ExternalApis.IntegrationTests`.




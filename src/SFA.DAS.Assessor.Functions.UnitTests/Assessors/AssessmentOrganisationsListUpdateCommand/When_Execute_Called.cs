using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Assessors.AssessmentOrganisationsListUpdateCommand
{
    public class When_Execute_Called
    {
        private Domain.Assessors.AssessmentOrganisationsListUpdateCommand _sut;
        private Mock<IAssessorServiceApiClient> _assessorServiceApiClient;

        [SetUp]
        public void Arrange()
        {
            var logger = new Mock<ILogger<Domain.Assessors.AssessmentOrganisationsListUpdateCommand>>();
            _assessorServiceApiClient = new Mock<IAssessorServiceApiClient>();

            _sut = new Domain.Assessors.AssessmentOrganisationsListUpdateCommand(logger.Object, _assessorServiceApiClient.Object);
        }

        [Test]
        public async Task ThenItShouldUpdateTheAssessmentOrganisationsList()
        {
            // Act
            await _sut.Execute();

            // Assert
            _assessorServiceApiClient.Verify(p => p.UpdateAssessmentOrganisationsList(), Times.Once());
        }
    }
}

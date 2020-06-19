﻿using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;

namespace SFA.DAS.Assessor.Functions.UnitTests.Standards.StandardCollationImportCommand
{
    public class WhenCommandIsExecuted
    {
        private Domain.Standards.StandardCollationImportCommand _sut;
        private Mock<IAssessorServiceApiClient> _assessorServiceApiClient;

        [SetUp]
        public void Arrange()
        {
            var logger = new Mock<ILogger<Domain.Standards.StandardCollationImportCommand>>();
            _assessorServiceApiClient = new Mock<IAssessorServiceApiClient>();

            _sut = new Domain.Standards.StandardCollationImportCommand(logger.Object, _assessorServiceApiClient.Object);
        }

        [Test]
        public void ThenItShouldUpdateStandards()
        {
            // Act
            _sut.Execute();

            // Assert
            _assessorServiceApiClient.Verify(p => p.UpdateStandards(), Times.Once());
        }
    }
}

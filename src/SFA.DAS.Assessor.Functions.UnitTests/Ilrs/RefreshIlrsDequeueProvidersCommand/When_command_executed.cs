﻿using Microsoft.Azure.Functions.Worker;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using SFA.DAS.Assessor.Functions.Infrastructure.Queues;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ilrs.RefreshIlrsDequeueProvidersCommand
{
    public class When_command_executed
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(99)]
        public async Task Then_process_learner_recieves_incomming_message(int pageNumber)
        {
            // Arrange
            Mock<IRefreshIlrsLearnerService> refreshIlrsLearnerService = new Mock<IRefreshIlrsLearnerService>();
            Mock<IQueueService> queueServiceMock = new Mock<IQueueService>();

            Domain.Ilrs.RefreshIlrsDequeueProvidersCommand sut = new Domain.Ilrs.RefreshIlrsDequeueProvidersCommand(refreshIlrsLearnerService.Object, queueServiceMock.Object);

            JObject inputQueueMessage = new JObject
            {
                { "Source", "1920" },
                { "Ukprn", "222222" },
                { "LearnerPageNumber", pageNumber }
            };

            // Act
            await sut.Execute(inputQueueMessage.ToString());

            // Assert            
            refreshIlrsLearnerService.Verify(p => p.ProcessLearners(It.Is<RefreshIlrsProviderMessage>(m => m.Equals(JsonConvert.DeserializeObject<RefreshIlrsProviderMessage>(inputQueueMessage.ToString())))));
        }
    }
}

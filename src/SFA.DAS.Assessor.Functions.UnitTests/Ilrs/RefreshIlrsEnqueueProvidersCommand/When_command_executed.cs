﻿using Microsoft.Azure.WebJobs;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ilrs.RefreshIlrsEnqueueProvidersCommand
{
    public class When_command_executed
    {
        Mock<IRefreshIlrsProviderService> _refreshIlrsProviderService = new Mock<IRefreshIlrsProviderService>();
        Mock<ICollector<string>> _storageQueue = new Mock<ICollector<string>>();
        Mock<IDateTimeHelper> _dateTimeHelper = new Mock<IDateTimeHelper>();
        
        Domain.Ilrs.RefreshIlrsEnqueueProvidersCommand _sut;

        RefreshIlrsProviderMessage provider1 = new RefreshIlrsProviderMessage
        {
            Ukprn = 111111,
            Source = "1920",
            LearnerPageNumber = 1
        };

        RefreshIlrsProviderMessage provider2 = new RefreshIlrsProviderMessage
        {
            Ukprn = 222222,
            Source = "1920",
            LearnerPageNumber = 1
        };

        RefreshIlrsProviderMessage provider3 = new RefreshIlrsProviderMessage
        {
            Ukprn = 3333333,
            Source = "2021",
            LearnerPageNumber = 1
        };

        [SetUp]
        public void Arrange()
        {
            // Arrange
            _sut = new Domain.Ilrs.RefreshIlrsEnqueueProvidersCommand(
                    _refreshIlrsProviderService.Object, _dateTimeHelper.Object)
            {
                StorageQueue = _storageQueue.Object
            };
        }
        
        [Test]
        public async Task Then_process_provider_is_called()
        {
            // Arrange
            Arrange();

            _refreshIlrsProviderService.Setup(p => p.ProcessProviders()).ReturnsAsync(
                new List<RefreshIlrsProviderMessage>());

            // Act
            await _sut.Execute();

            // Assert            
            _refreshIlrsProviderService.Verify(p => p.ProcessProviders(), Times.Once());
        }

        [Test]
        public async Task Then_storage_queue_received_provider()
        {
            // Arrange
            Arrange();

            _refreshIlrsProviderService.Setup(p => p.ProcessProviders()).ReturnsAsync(
                new List<RefreshIlrsProviderMessage>
                {
                    provider1,
                    provider2,
                    provider3
                });

            // Act
            await _sut.Execute();

            // Assert
            _storageQueue.Verify(p => p.Add(It.Is<string>(m => MessageEquals(m, JsonConvert.SerializeObject(provider1)))));
            _storageQueue.Verify(p => p.Add(It.Is<string>(m => MessageEquals(m, JsonConvert.SerializeObject(provider2)))));
            _storageQueue.Verify(p => p.Add(It.Is<string>(m => MessageEquals(m, JsonConvert.SerializeObject(provider3)))));
        }

        private bool MessageEquals(string first, string second)
        {
            var firstMessage = JsonConvert.DeserializeObject<RefreshIlrsProviderMessage>(first);
            var secondMessage = JsonConvert.DeserializeObject<RefreshIlrsProviderMessage>(second);

            return firstMessage.Equals(secondMessage);
        }
    }
}

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Queues;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ilrs.RefreshIlrsEnqueueProvidersCommand
{
    public class When_command_executed
    {
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
        
        [Test]
        public async Task Then_process_provider_is_called()
        {
            // Arrange
            var lastRunDateTime = new DateTime(2021, 5, 1);
            var dateTimeNow = new DateTime(2021, 6, 1);

            var testFixture = new TestFixture()
                .WithProviders(new List<RefreshIlrsProviderMessage>
                {
                    provider1,
                    provider2,
                    provider3
                })
                .WithLastRunDate(lastRunDateTime)
                .WithDateTimeNow(dateTimeNow)
                .Setup();

            // Act
            await testFixture.Execute();

            // Assert
            testFixture.VerifyProcessProviderCalled();
        }

        [Test]
        public async Task Then_no_providers_queued_does_not_update_last_run_date()
        {
            var lastRunDateTime = new DateTime(2021, 5, 1);
            var dateTimeNow = new DateTime(2021, 6, 1);

            var testFixture = new TestFixture()
                .WithProviders(new List<RefreshIlrsProviderMessage>())
                .WithLastRunDate(lastRunDateTime)
                .WithDateTimeNow(dateTimeNow)
                .Setup();

            // Act
            await testFixture.Execute();

            // Assert
            testFixture.VerifySetLastRunDateTimeCalled(dateTimeNow, Times.Never);
        }

        [Test]
        public async Task Then_providers_queued_storage_queue_received_provider()
        {
            // Arrange
            var lastRunDateTime = new DateTime(2021, 5, 1);
            var dateTimeNow = new DateTime(2021, 6, 1);

            var testFixture = new TestFixture()
                .WithProviders(new List<RefreshIlrsProviderMessage>
                {
                    provider1,
                    provider2,
                    provider3
                })
                .WithLastRunDate(lastRunDateTime)
                .WithDateTimeNow(dateTimeNow)
                .Setup();

            // Act
            await testFixture.Execute();

            // Assert
            testFixture.VerifyProviderAddedToStorageQueue(provider1);
            testFixture.VerifyProviderAddedToStorageQueue(provider2);
            testFixture.VerifyProviderAddedToStorageQueue(provider3);
        }

        [Test]
        public async Task Then_providers_queued_does_update_last_run_date_to_current_datetime()
        {
            var lastRunDateTime = new DateTime(2021, 5, 1);
            var dateTimeNow = new DateTime(2021, 6, 1);

            var testFixture = new TestFixture()
                .WithProviders(new List<RefreshIlrsProviderMessage>
                {
                    provider1,
                    provider2,
                    provider3
                })
                .WithLastRunDate(lastRunDateTime)
                .WithDateTimeNow(dateTimeNow)
                .Setup();
            
            // Act
            await testFixture.Execute();

            // Assert
            testFixture.VerifySetLastRunDateTimeCalled(dateTimeNow, Times.Once);
        }


        private class TestFixture
        {
            public Mock<IRefreshIlrsAccessorSettingService> RefreshIlrsAccessorSettingService = new Mock<IRefreshIlrsAccessorSettingService>();
            public Mock<IRefreshIlrsProviderService> RefreshIlrsProviderService = new Mock<IRefreshIlrsProviderService>();
            public Mock<IQueueService> QueueService = new Mock<IQueueService>();
            public Mock<IDateTimeHelper> DateTimeHelper = new Mock<IDateTimeHelper>();
            public Mock<ILogger<Domain.Ilrs.RefreshIlrsEnqueueProvidersCommand>> Logger = new Mock<ILogger<Domain.Ilrs.RefreshIlrsEnqueueProvidersCommand>>();

            public Domain.Ilrs.RefreshIlrsEnqueueProvidersCommand Sut;

            public TestFixture Setup()
            {
                Sut = new Domain.Ilrs.RefreshIlrsEnqueueProvidersCommand(
                    RefreshIlrsAccessorSettingService.Object, RefreshIlrsProviderService.Object, 
                    DateTimeHelper.Object, QueueService.Object, Logger.Object );

                return this;
            }

            public async Task Execute()
            {
                await Sut.Execute();
            }

            public TestFixture WithProviders(List<RefreshIlrsProviderMessage> providers)
            {
                RefreshIlrsProviderService.Setup(p => p.ProcessProviders(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                    .ReturnsAsync(providers);

                return this;
            }

            public TestFixture WithLastRunDate(DateTime lastRunDateTime)
            {
                RefreshIlrsAccessorSettingService.Setup(p => p.GetLastRunDateTime())
                    .ReturnsAsync(lastRunDateTime);

                return this;
            }

            public TestFixture WithDateTimeNow(DateTime dateTimeNow)
            { 
                DateTimeHelper.Setup(p => p.DateTimeNow)
                    .Returns(dateTimeNow);

                return this;
            }

            public void VerifySetLastRunDateTimeCalled(DateTime dateTime, Func<Times> times)
            {
                RefreshIlrsAccessorSettingService.Verify(p => p.SetLastRunDateTime(dateTime), times);
            }

            public void VerifyProviderAddedToStorageQueue(RefreshIlrsProviderMessage provider)
            {
                QueueService.Verify(p => p.EnqueueMessageAsync(QueueNames.RefreshIlrs, It.Is<RefreshIlrsProviderMessage>(m => m.Equals(provider))));
            }

            public void VerifyProcessProviderCalled()
            {
                RefreshIlrsProviderService.Verify(p => p.ProcessProviders(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once());
            }
        }
    }
}

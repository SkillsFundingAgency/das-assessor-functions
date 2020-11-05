﻿using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.UnitTests.AssessorServiceApi
{
    public class When_UpdateBatchLogPrinted_Called : AssessorServiceApiTestBase
    {
        [SetUp]
        public override void Arrange()
        {
            base.Arrange();
        }

        [Test]
        public async Task Then_ApiCalled_ToUpdateBatchLogPrinted()
        {
            // Arrange
            var batchNumber = 111;
            
            var validationResponse = Builder<ValidationResponse>.CreateNew().Build();
            var request = Builder<UpdateBatchLogPrintedRequest>.CreateNew().Build();

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>
                (
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.AbsolutePath == $"/api/v1/batches/{batchNumber}/update-printed" && r.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(validationResponse), Encoding.UTF8, "text/json") })
                .Verifiable();

            // Act
            var result = await _sut.UpdateBatchLogPrinted(batchNumber, request);

            // Assert
            result.Should().Equals(validationResponse);
        }
    }
}

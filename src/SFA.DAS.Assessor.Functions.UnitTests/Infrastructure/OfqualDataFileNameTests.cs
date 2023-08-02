using System;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.UnitTests.Infrastructure
{
    public class OfqualDataFileNameTests
    {
        [TestCase(OfqualDataType.Organisations)]
        [TestCase(OfqualDataType.Qualifications)]
        public void CreateForFileType_GeneratesFileName_WithTypeAsPrefixAndDateAsSuffix(OfqualDataType fileType)
        {
            string expected = $"{fileType}_export_{DateTime.UtcNow.ToString("yyyyMMdd")}.csv";

            var result = OfqualDataFileName.CreateForFileType(fileType);
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}

using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.PrintExtensionFunction
{
    public class StringNameCaseExtensionTests
    {
        [TestCase("mcgregor", "McGregor")]
        [TestCase("mcLean", "McLean")]
        [TestCase("McDonald", "McDonald")]
        [TestCase("hemchandra", "Hemchandra")]
        [TestCase("szymczyk", "Szymczyk")]
        [TestCase("Maciejewska", "Maciejewska")]
        [TestCase("MACHACEk", "Machacek")]
        public void ThenNameShouldBeCapitalized(string name, string expectedAfterCapitalization)
        {
            var nameAfterCapitalization = name.ProperCase();

            Assert.AreEqual(expectedAfterCapitalization, nameAfterCapitalization);
        }
    }
}

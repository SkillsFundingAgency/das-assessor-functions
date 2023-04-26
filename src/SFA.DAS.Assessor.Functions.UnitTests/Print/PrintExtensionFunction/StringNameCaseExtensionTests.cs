using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.PrintExtensionFunction
{
    public class StringNameCaseExtensionTests
    {
        [TestCase("McGregor", "McGregor")]
        [TestCase("McLean", "McLean")]
        [TestCase("McDonald", "McDonald")]
        [TestCase("HemChandra", "Hemchandra")]
        [TestCase("Szymczyk", "Szymczyk")]
        [TestCase("MacIejewska", "Maciejewska")]
        [TestCase("MacHacek", "Machacek")]
        [TestCase("MacUgova", "Macugova")]
        [TestCase("MacHajewski", "Machajewski")]
        [TestCase("MacIazek", "Maciazek")]
        [TestCase("MacHniak", "Machniak")]
        public void ThenFamilyNameShouldBeCapitalizedCorrectly(string familyName, string expectedToProperCase)
        {
            // Arrange
            var upperCase = familyName.ToUpper();
            var lowerCase = familyName.ToLower();

            // Act
            var upperCaseToProperCase = upperCase.ProperCase(true);
            var lowerCaseToProperCase = lowerCase.ProperCase(true);
            var familyNameToProperCase = familyName.ProperCase(true);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedToProperCase, upperCaseToProperCase, "Failed to proper case an upper case family name");
                Assert.AreEqual(expectedToProperCase, lowerCaseToProperCase, "Failed to proper case an lower case family name");
                Assert.AreEqual(expectedToProperCase, familyNameToProperCase, "Failed to proper case a mixed case family name");
            });
        }
    }
}

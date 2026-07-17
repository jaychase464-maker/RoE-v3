using System.Linq;
using NUnit.Framework;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone6;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class MilestoneSixHeadquartersConfigurationTests
    {
        [Test]
        public void MilestoneSixHeadquartersValidator_HasNoErrorsAfterSetup()
        {
            ProjectValidationResult[] errors =
                RulesOfEntryMilestoneSixHeadquartersValidator
                    .RunValidation(false)
                    .Where(result =>
                        result.Severity == ProjectValidationSeverity.Error)
                    .ToArray();
            string details = string.Join(
                "\n",
                errors.Select(error => $"{error.Check}: {error.Message}"));
            Assert.That(errors, Is.Empty, details);
        }
    }
}

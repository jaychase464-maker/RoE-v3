using System.Linq;
using NUnit.Framework;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone2;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class MilestoneTwoConfigurationTests
    {
        [Test]
        public void MilestoneTwoValidator_HasNoErrorsAfterSetup()
        {
            ProjectValidationResult[] errors = RulesOfEntryMilestoneTwoValidator
                .RunValidation(false)
                .Where(result => result.Severity == ProjectValidationSeverity.Error)
                .ToArray();
            string details = string.Join(
                "\n",
                errors.Select(error => $"{error.Check}: {error.Message}"));
            Assert.That(errors, Is.Empty, details);
        }
    }
}

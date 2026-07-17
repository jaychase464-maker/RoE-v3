using System.Linq;
using NUnit.Framework;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone4;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class MilestoneFourConfigurationTests
    {
        [Test]
        public void MilestoneFourValidator_HasNoErrorsAfterSetup()
        {
            ProjectValidationResult[] errors = RulesOfEntryMilestoneFourValidator
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

using System.Linq;
using NUnit.Framework;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone3;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class MilestoneThreeConfigurationTests
    {
        [Test]
        public void MilestoneThreeValidator_HasNoErrorsAfterSetup()
        {
            ProjectValidationResult[] errors = RulesOfEntryMilestoneThreeValidator
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

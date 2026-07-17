using System.Linq;
using NUnit.Framework;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone1;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class MilestoneOneConfigurationTests
    {
        [Test]
        public void MilestoneOneValidator_HasNoErrorsAfterSetup()
        {
            ProjectValidationResult[] errors = RulesOfEntryMilestoneOneValidator
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

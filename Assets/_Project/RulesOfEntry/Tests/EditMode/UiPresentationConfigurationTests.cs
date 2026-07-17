using System.Linq;
using NUnit.Framework;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.UiPresentation;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class UiPresentationConfigurationTests
    {
        [Test]
        public void UiPresentationValidator_HasNoErrorsAfterSetup()
        {
            ProjectValidationResult[] errors = RulesOfEntryUiPresentationValidator
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

using System.Linq;
using NUnit.Framework;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class ProjectFoundationTests
    {
        [Test]
        public void ProjectIdentity_MatchesMilestoneZeroContract()
        {
            Assert.That(ProjectInfo.GameTitle, Is.EqualTo("Rules of Entry"));
            Assert.That(ProjectInfo.ProjectCode, Is.EqualTo("ROE"));
            Assert.That(ProjectInfo.ExpectedUnityVersion, Is.EqualTo("6000.5.2f1"));
            Assert.That(ProjectInfo.FoundationSchemaVersion, Is.EqualTo(1));
        }

        [Test]
        public void ProjectValidator_HasNoErrorsAfterFoundationSetup()
        {
            ProjectValidationResult[] errors = RulesOfEntryProjectValidator
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

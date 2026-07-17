using System.Linq;
using RulesOfEntry.Editor.Foundation;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RulesOfEntry.Editor.Milestone3
{
    public sealed class RulesOfEntryMilestoneThreeBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -700;

        public void OnPreprocessBuild(BuildReport report)
        {
            ProjectValidationResult[] errors = RulesOfEntryMilestoneThreeValidator
                .RunValidation(false)
                .Where(result => result.Severity == ProjectValidationSeverity.Error)
                .ToArray();
            if (errors.Length == 0)
            {
                return;
            }

            string details = string.Join(
                "\n",
                errors.Select(error => $"- {error.Check}: {error.Message}"));
            throw new BuildFailedException(
                "Rules of Entry Milestone 3 validation failed:\n" + details);
        }
    }
}

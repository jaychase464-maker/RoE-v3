using System.Linq;
using RulesOfEntry.Editor.Foundation;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RulesOfEntry.Editor.Milestone4
{
    public sealed class RulesOfEntryMilestoneFourBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -600;

        public void OnPreprocessBuild(BuildReport report)
        {
            ProjectValidationResult[] errors = RulesOfEntryMilestoneFourValidator
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
                "Rules of Entry Milestone 4 validation failed:\n" + details);
        }
    }
}

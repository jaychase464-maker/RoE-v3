using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RulesOfEntry.Editor.Milestone2
{
    public sealed class RulesOfEntryMilestoneTwoBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -800;

        public void OnPreprocessBuild(BuildReport report)
        {
            ProjectValidationResult[] errors = RulesOfEntryMilestoneTwoValidator
                .RunValidation(false)
                .Where(result => result.Severity == ProjectValidationSeverity.Error)
                .ToArray();
            if (errors.Length == 0)
            {
                ProjectLog.Info("Build Validation", "Milestone 2 validation passed.");
                return;
            }

            string details = string.Join(
                "\n",
                errors.Select(error => $"- {error.Check}: {error.Message}"));
            throw new BuildFailedException(
                $"Rules of Entry build blocked by {errors.Length} Milestone 2 validation error(s):\n{details}");
        }
    }
}

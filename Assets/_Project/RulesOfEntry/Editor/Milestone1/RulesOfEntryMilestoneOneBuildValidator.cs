using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RulesOfEntry.Editor.Milestone1
{
    /// <summary>
    /// Prevents a build when the playable Milestone 1 contract is incomplete.
    /// </summary>
    public sealed class RulesOfEntryMilestoneOneBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -900;

        public void OnPreprocessBuild(BuildReport report)
        {
            ProjectValidationResult[] errors = RulesOfEntryMilestoneOneValidator
                .RunValidation(false)
                .Where(result => result.Severity == ProjectValidationSeverity.Error)
                .ToArray();

            if (errors.Length == 0)
            {
                ProjectLog.Info("Build Validation", "Milestone 1 validation passed.");
                return;
            }

            string details = string.Join(
                "\n",
                errors.Select(error => $"- {error.Check}: {error.Message}"));
            throw new BuildFailedException(
                $"Rules of Entry build blocked by {errors.Length} Milestone 1 validation error(s):\n{details}");
        }
    }
}

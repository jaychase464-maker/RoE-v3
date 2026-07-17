using System.Linq;
using RulesOfEntry.Core;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RulesOfEntry.Editor.Foundation
{
    /// <summary>
    /// Prevents a distributable build when foundation validation contains errors.
    /// </summary>
    public sealed class RulesOfEntryBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            ProjectValidationResult[] errors = RulesOfEntryProjectValidator
                .RunValidation(false)
                .Where(result => result.Severity == ProjectValidationSeverity.Error)
                .ToArray();

            if (errors.Length == 0)
            {
                ProjectLog.Info("Build Validation", "Foundation validation passed.");
                return;
            }

            string details = string.Join(
                "\n",
                errors.Select(error => $"- {error.Check}: {error.Message}"));
            throw new BuildFailedException(
                $"Rules of Entry build blocked by {errors.Length} validation error(s):\n{details}");
        }
    }
}

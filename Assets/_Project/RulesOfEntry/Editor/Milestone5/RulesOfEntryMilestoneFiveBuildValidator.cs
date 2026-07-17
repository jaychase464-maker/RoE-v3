using System;
using System.Linq;
using RulesOfEntry.Editor.Foundation;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RulesOfEntry.Editor.Milestone5
{
    public sealed class RulesOfEntryMilestoneFiveBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -480;

        public void OnPreprocessBuild(BuildReport report)
        {
            ProjectValidationResult[] errors = RulesOfEntryMilestoneFiveValidator
                .RunValidation(false)
                .Where(result => result.Severity == ProjectValidationSeverity.Error)
                .ToArray();
            if (errors.Length == 0)
            {
                return;
            }

            string details = string.Join(
                Environment.NewLine,
                errors.Select(error => $"{error.Check}: {error.Message}"));
            throw new BuildFailedException(
                "Rules of Entry Milestone 5 validation failed before build:"
                    + Environment.NewLine
                    + details);
        }
    }
}

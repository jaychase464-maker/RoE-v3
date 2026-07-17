using System;
using System.Linq;
using RulesOfEntry.Editor.Foundation;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RulesOfEntry.Editor.UiPresentation
{
    public sealed class RulesOfEntryUiPresentationBuildValidator :
        IPreprocessBuildWithReport
    {
        public int callbackOrder => -470;

        public void OnPreprocessBuild(BuildReport report)
        {
            ProjectValidationResult[] errors = RulesOfEntryUiPresentationValidator
                .RunValidation(false)
                .Where(result =>
                    result.Severity == ProjectValidationSeverity.Error)
                .ToArray();
            if (errors.Length == 0)
            {
                return;
            }

            string details = string.Join(
                Environment.NewLine,
                errors.Select(error => $"{error.Check}: {error.Message}"));
            throw new BuildFailedException(
                "Rules of Entry UI Presentation validation failed before build:"
                    + Environment.NewLine
                    + details);
        }
    }
}

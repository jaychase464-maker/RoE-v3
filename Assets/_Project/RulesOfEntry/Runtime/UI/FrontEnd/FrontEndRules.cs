using UnityEngine;

namespace RulesOfEntry.UI.FrontEnd
{
    /// <summary>
    /// Pure front-end rules kept separate from scene loading and presentation.
    /// </summary>
    public static class FrontEndRules
    {
        public static int GetNextQualityIndex(int currentIndex, int qualityLevelCount)
        {
            if (qualityLevelCount <= 0)
            {
                return 0;
            }

            int normalized = Mathf.Clamp(currentIndex, 0, qualityLevelCount - 1);
            return (normalized + 1) % qualityLevelCount;
        }

        public static float NormalizeLoadingProgress(float sceneLoadingProgress)
        {
            return Mathf.Clamp01(sceneLoadingProgress / 0.9f);
        }

        public static bool IsWarningContinueRequested(
            bool enterPressed,
            bool numpadEnterPressed,
            bool gamepadSouthPressed)
        {
            return enterPressed || numpadEnterPressed || gamepadSouthPressed;
        }
    }
}

using UnityEngine;

namespace RulesOfEntry.Operations
{
    /// <summary>
    /// Clears editor-persistent statics at application startup when Domain Reload is disabled.
    /// Scene transitions do not invoke this hook, so an operation record survives the HQ return.
    /// </summary>
    internal static class CompletedOperationContextRuntimeReset
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForApplicationSession()
        {
            CompletedOperationContext.Clear();
        }
    }
}

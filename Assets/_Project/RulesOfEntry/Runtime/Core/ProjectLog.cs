using System;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RulesOfEntry.Core
{
    /// <summary>
    /// Consistent logging entry point for project-owned systems.
    /// </summary>
    public static class ProjectLog
    {
        private const string Prefix = "[Rules of Entry]";

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Development(string system, string message, Object context = null)
        {
            UnityEngine.Debug.Log(Format(system, message), context);
        }

        public static void Info(string system, string message, Object context = null)
        {
            UnityEngine.Debug.Log(Format(system, message), context);
        }

        public static void Warning(string system, string message, Object context = null)
        {
            UnityEngine.Debug.LogWarning(Format(system, message), context);
        }

        public static void Error(string system, string message, Object context = null)
        {
            UnityEngine.Debug.LogError(Format(system, message), context);
        }

        public static void Exception(string system, Exception exception, Object context = null)
        {
            if (exception == null)
            {
                Error(system, "An exception was reported without an exception object.", context);
                return;
            }

            UnityEngine.Debug.LogError(Format(system, exception.Message), context);
            UnityEngine.Debug.LogException(exception, context);
        }

        private static string Format(string system, string message)
        {
            string safeSystem = string.IsNullOrWhiteSpace(system) ? "General" : system.Trim();
            string safeMessage = string.IsNullOrWhiteSpace(message) ? "No message supplied." : message.Trim();
            return $"{Prefix}[{safeSystem}] {safeMessage}";
        }
    }
}

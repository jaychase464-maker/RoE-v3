using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class UnitySixApiCompatibilityTests
    {
        [Test]
        public void RuntimeCode_DoesNotCallDeprecatedGetInstanceId()
        {
            string runtimeRoot = Path.Combine(
                Application.dataPath,
                "_Project",
                "RulesOfEntry",
                "Runtime");

            Assert.That(Directory.Exists(runtimeRoot), Is.True, $"Runtime folder was not found: {runtimeRoot}");

            foreach (string filePath in Directory.EnumerateFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories))
            {
                string source = File.ReadAllText(filePath);
                Assert.That(
                    source.Contains("GetInstanceID("),
                    Is.False,
                    $"Unity 6000.5 makes Object.GetInstanceID() a compile error. Use Object.GetEntityId() and preserve its full 64-bit value. File: {filePath}");
            }
        }
    }
}

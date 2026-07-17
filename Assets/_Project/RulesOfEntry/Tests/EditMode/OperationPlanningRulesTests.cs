using System;
using NUnit.Framework;
using RulesOfEntry.Missions;
using RulesOfEntry.Planning;
using UnityEngine;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class OperationPlanningRulesTests
    {
        private MissionDefinition mission;
        private OperationBriefingDefinition briefing;

        [TearDown]
        public void TearDown()
        {
            OperationDeploymentContext.Clear();
            if (briefing != null)
            {
                UnityEngine.Object.DestroyImmediate(briefing);
            }

            if (mission != null)
            {
                UnityEngine.Object.DestroyImmediate(mission);
            }
        }

        [TestCase(0, -1, 3, 2)]
        [TestCase(2, 1, 3, 0)]
        [TestCase(-1, 0, 3, 2)]
        [TestCase(4, 0, 3, 1)]
        [TestCase(0, 1, 0, -1)]
        public void WrapIndex_ProducesStableCircularSelection(
            int current,
            int direction,
            int count,
            int expected)
        {
            Assert.That(
                OperationPlanningRules.WrapIndex(current, direction, count),
                Is.EqualTo(expected));
        }

        [Test]
        public void CanDeploy_RequiresValidEntryAndAvailableOfficer()
        {
            CreateBriefing();

            Assert.That(
                OperationPlanningRules.CanDeploy(
                    briefing,
                    "entry_front",
                    new[] { "officer_alpha" }),
                Is.True);
            Assert.That(
                OperationPlanningRules.CanDeploy(
                    briefing,
                    "entry_missing",
                    new[] { "officer_alpha" }),
                Is.False);
            Assert.That(
                OperationPlanningRules.CanDeploy(
                    briefing,
                    "entry_front",
                    Array.Empty<string>()),
                Is.False);
        }

        [Test]
        public void DeploymentContext_StoresIdentifiersAndRejectsFutureSupport()
        {
            CreateBriefing();

            bool confirmed = OperationDeploymentContext.Confirm(
                briefing,
                "entry_front",
                new[] { "officer_alpha" },
                new[] { "support_k9" });

            Assert.That(confirmed, Is.True);
            Assert.That(OperationDeploymentContext.HasPendingDeployment, Is.True);
            Assert.That(OperationDeploymentContext.MissionId, Is.EqualTo("test_mission"));
            Assert.That(OperationDeploymentContext.EntryPointId, Is.EqualTo("entry_front"));
            CollectionAssert.AreEqual(
                new[] { "officer_alpha" },
                OperationDeploymentContext.AssignedOfficerIds);
            Assert.That(OperationDeploymentContext.SupportAssetIds, Is.Empty);
        }

        private void CreateBriefing()
        {
            MissionObjectiveDefinition objective = new MissionObjectiveDefinition();
            objective.Configure(
                "test_objective",
                "Test objective",
                string.Empty,
                MissionObjectiveType.PreserveOfficerTeam,
                string.Empty,
                string.Empty,
                true,
                20);
            mission = ScriptableObject.CreateInstance<MissionDefinition>();
            mission.Configure(
                "test_mission",
                "Test Mission",
                string.Empty,
                new[] { objective });

            OperationEntryPointDefinition entry = new OperationEntryPointDefinition();
            entry.Configure("entry_front", "Front Entry", string.Empty, string.Empty);
            OperationOfficerDefinition officer = new OperationOfficerDefinition();
            officer.Configure(
                "officer_alpha",
                "Officer Alpha",
                "Lead",
                "General tactical",
                true,
                true);
            OperationSupportDefinition support = new OperationSupportDefinition();
            support.Configure(
                "support_k9",
                "K9 Team",
                OperationSupportType.K9,
                "Search support",
                false,
                false);

            briefing = ScriptableObject.CreateInstance<OperationBriefingDefinition>();
            briefing.Configure(
                "TEST 01",
                mission,
                "Assets/Test.unity",
                "Test Site",
                "Test Incident",
                "Now",
                "Clear",
                "Test intelligence",
                "Test authority",
                "Test ROE",
                new[] { entry },
                new[] { officer },
                new[] { support });
        }
    }
}

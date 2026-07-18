using System;
using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.Officers;
using RulesOfEntry.UI.TacticalHud;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class TacticalHudRulesTests
    {
        [TestCase(0, OfficerAmmunitionCondition.Critical)]
        [TestCase(1, OfficerAmmunitionCondition.Low)]
        [TestCase(2, OfficerAmmunitionCondition.Good)]
        [TestCase(4, OfficerAmmunitionCondition.Good)]
        public void AmmunitionStatus_RemainsQualitative(
            int magazines,
            OfficerAmmunitionCondition expected)
        {
            Assert.That(
                OfficerAmmunitionStatus.EvaluateCondition(magazines),
                Is.EqualTo(expected));
        }

        [Test]
        public void ConditionLabels_DoNotExposeArcadeHealthValues()
        {
            Assert.That(
                TacticalHudRules.GetConditionLabel(ActorConditionLevel.Stable),
                Is.EqualTo("FIT"));
            Assert.That(
                TacticalHudRules.GetConditionLabel(ActorConditionLevel.Wounded),
                Is.EqualTo("WOUNDED"));
            Assert.That(
                TacticalHudRules.GetConditionLabel(ActorConditionLevel.Incapacitated),
                Is.EqualTo("DOWN"));
        }

        [Test]
        public void BodyCameraTimestamp_UsesMissionClockPresentation()
        {
            DateTime timestamp = new DateTime(2026, 7, 17, 22, 41, 7);
            Assert.That(
                TacticalHudRules.FormatBodyCameraTimestamp(timestamp),
                Is.EqualTo("17 JUL 2026   22:41:07"));
        }

        [TestCase(OfficerCommandTargetType.Position, 0)]
        [TestCase(OfficerCommandTargetType.Door, 2)]
        [TestCase(OfficerCommandTargetType.Subject, 5)]
        public void ContextSuggestsExpectedCommand(
            OfficerCommandTargetType targetType,
            int expectedIndex)
        {
            Assert.That(
                TacticalHudRules.GetSuggestedCommandIndex(targetType),
                Is.EqualTo(expectedIndex));
        }

        [TestCase(1, OfficerOrderType.MoveTo)]
        [TestCase(2, OfficerOrderType.HoldPosition)]
        [TestCase(3, OfficerOrderType.StackAtDoor)]
        [TestCase(4, OfficerOrderType.OpenDoor)]
        [TestCase(5, OfficerOrderType.Follow)]
        [TestCase(6, OfficerOrderType.RestrainSubject)]
        public void NumberSlotMapsToExpectedOfficerOrder(
            int slot,
            OfficerOrderType expected)
        {
            bool mapped = OfficerCommandSlotRules.TryGetOrderType(slot, out var orderType);
            Assert.That(mapped, Is.True);
            Assert.That(orderType, Is.EqualTo(expected));
        }

        [Test]
        public void InvalidNumberSlotDoesNotMapToAnOrder()
        {
            Assert.That(
                OfficerCommandSlotRules.TryGetOrderType(0, out _),
                Is.False);
            Assert.That(
                OfficerCommandSlotRules.TryGetOrderType(7, out _),
                Is.False);
        }
    }
}

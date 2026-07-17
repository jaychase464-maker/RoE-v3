using NUnit.Framework;
using RulesOfEntry.Officers;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class OfficerOrderStateMachineTests
    {
        [Test]
        public void AcceptedOrder_RequiresExecutionBeforeCompletion()
        {
            OfficerOrderStateMachine state = new OfficerOrderStateMachine();

            Assert.That(state.TryComplete(), Is.False);
            Assert.That(state.TryAccept(), Is.True);
            Assert.That(state.TryComplete(), Is.False);
            Assert.That(state.TryBeginExecution(), Is.True);
            Assert.That(state.TryComplete("Reached position."), Is.True);

            Assert.That(state.Status, Is.EqualTo(OfficerOrderStatus.Completed));
            Assert.That(state.OutcomeReason, Is.EqualTo(OfficerOrderOutcomeReason.None));
            Assert.That(state.IsTerminal, Is.True);
        }

        [Test]
        public void ActiveOrder_CanBeCancelledExactlyOnce()
        {
            OfficerOrderStateMachine state = new OfficerOrderStateMachine();
            Assert.That(state.TryAccept(), Is.True);
            Assert.That(state.TryBeginExecution(), Is.True);

            Assert.That(
                state.TryCancel(
                    OfficerOrderOutcomeReason.CancelledByPlayer,
                    "Player changed plan."),
                Is.True);
            Assert.That(
                state.TryCancel(OfficerOrderOutcomeReason.CancelledByPlayer),
                Is.False);
            Assert.That(state.Status, Is.EqualTo(OfficerOrderStatus.Cancelled));
            Assert.That(
                state.OutcomeReason,
                Is.EqualTo(OfficerOrderOutcomeReason.CancelledByPlayer));
        }

        [Test]
        public void Failure_PreservesSpecificCauseAndDetails()
        {
            OfficerOrderStateMachine state = new OfficerOrderStateMachine();
            Assert.That(state.TryAccept(), Is.True);

            Assert.That(
                state.TryFail(
                    OfficerOrderOutcomeReason.NoPath,
                    "Door-side point is outside the baked surface."),
                Is.True);
            Assert.That(state.Status, Is.EqualTo(OfficerOrderStatus.Failed));
            Assert.That(state.OutcomeReason, Is.EqualTo(OfficerOrderOutcomeReason.NoPath));
            Assert.That(state.Details, Does.Contain("baked surface"));
            Assert.That(state.TryBeginExecution(), Is.False);
        }

        [Test]
        public void Refusal_IsOnlyValidBeforeAcceptance()
        {
            OfficerOrderStateMachine refused = new OfficerOrderStateMachine();
            Assert.That(
                refused.TryRefuse(
                    OfficerOrderOutcomeReason.OfficerIncapacitated,
                    "Unable to act."),
                Is.True);
            Assert.That(refused.Status, Is.EqualTo(OfficerOrderStatus.Refused));

            OfficerOrderStateMachine accepted = new OfficerOrderStateMachine();
            Assert.That(accepted.TryAccept(), Is.True);
            Assert.That(
                accepted.TryRefuse(
                    OfficerOrderOutcomeReason.OfficerUnavailable,
                    "Too late."),
                Is.False);
        }
    }
}

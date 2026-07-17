using NUnit.Framework;
using RulesOfEntry.Actors;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class CustodyStateMachineTests
    {
        [Test]
        public void CooperativeCustodyPath_RequiresEveryProceduralStep()
        {
            CustodyStateMachine state = new CustodyStateMachine();

            Assert.That(state.TryBeginSurrender(), Is.True);
            Assert.That(state.State, Is.EqualTo(CustodyState.Surrendering));
            Assert.That(state.TryOrderToKneel(), Is.True);
            Assert.That(state.TryApplyRestraints(), Is.True);
            Assert.That(state.TryMarkSearched(), Is.True);
            Assert.That(state.TryTransferToCustody(), Is.True);
            Assert.That(state.State, Is.EqualTo(CustodyState.InCustody));
        }

        [Test]
        public void FreeSubject_CannotBeInstantlyHandcuffedOrSearched()
        {
            CustodyStateMachine state = new CustodyStateMachine();

            Assert.That(state.TryApplyRestraints(), Is.False);
            Assert.That(state.TryMarkSearched(), Is.False);
            Assert.That(state.State, Is.EqualTo(CustodyState.Free));
        }

        [Test]
        public void RestrainedSubject_CannotBreakSurrender()
        {
            CustodyStateMachine state = new CustodyStateMachine();
            state.TryBeginSurrender();
            state.TryOrderToKneel();
            state.TryApplyRestraints();

            Assert.That(state.TryBreakSurrender(), Is.False);
            Assert.That(state.State, Is.EqualTo(CustodyState.Restrained));
        }
    }
}

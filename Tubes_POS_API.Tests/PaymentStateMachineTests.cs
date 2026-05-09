using Tubes_POS_API.Entities.Enums;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class PaymentStateMachineTests
{
    [Fact]
    public void InitialState_ShouldBeCreated()
    {
        var machine = new PaymentStateMachine();

        Assert.Equal(PaymentStatus.Created, machine.CurrentState);
    }

    [Fact]
    public void MarkPaid_ShouldMoveToPaid()
    {
        var machine = new PaymentStateMachine();

        machine.MarkPaid();

        Assert.Equal(PaymentStatus.Paid, machine.CurrentState);
    }

    [Fact]
    public void Complete_AfterPaid_ShouldMoveToCompleted()
    {
        var machine = new PaymentStateMachine();

        machine.MarkPaid();
        machine.Complete();

        Assert.Equal(PaymentStatus.Completed, machine.CurrentState);
    }

    [Fact]
    public void Complete_WithoutPaid_ShouldThrow()
    {
        var machine = new PaymentStateMachine();

        Assert.Throws<InvalidOperationException>(() => machine.Complete());
    }
}

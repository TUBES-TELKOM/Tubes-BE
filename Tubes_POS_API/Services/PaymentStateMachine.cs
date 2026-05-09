using Tubes_POS_API.Entities.Enums;

namespace Tubes_POS_API.Services;

public sealed class PaymentStateMachine
{
    public PaymentStatus CurrentState { get; private set; } = PaymentStatus.Created;

    public void MarkPaid()
    {
        EnsureCurrentState(PaymentStatus.Created);
        CurrentState = PaymentStatus.Paid;
    }

    public void Complete()
    {
        EnsureCurrentState(PaymentStatus.Paid);
        CurrentState = PaymentStatus.Completed;
    }

    public void Fail()
    {
        CurrentState = PaymentStatus.Failed;
    }

    private void EnsureCurrentState(PaymentStatus required)
    {
        if (CurrentState != required)
        {
            throw new InvalidOperationException($"Transisi payment tidak valid dari {CurrentState} ke {required}.");
        }
    }
}

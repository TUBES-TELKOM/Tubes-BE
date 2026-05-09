namespace Tubes_POS_API
{
        public enum PaymentState
        {
            WaitingPayment,
            Paid,
            Failed
        }

        public class StateMachine
        {
            public PaymentState CurrentState { get; private set; }

            public StateMachine()
            {
                CurrentState = PaymentState.WaitingPayment;
            }

            public void Process(decimal total, decimal cash)
            {
                if (cash >= total)
                {
                    CurrentState = PaymentState.Paid;
                }
                else
                {
                    CurrentState = PaymentState.Failed;
                }
            }
        }
}

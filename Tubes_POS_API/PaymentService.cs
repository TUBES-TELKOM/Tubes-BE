namespace Tubes_POS_API
{
    // ===============================
    // GENERIC RESPONSE
    // ===============================
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    // ===============================
    // PAYMENT SERVICE
    // ===============================
    public class PaymentService
    {
        private readonly StateMachine _stateMachine;

        public PaymentService()
        {
            _stateMachine = new StateMachine();
        }

        public ApiResponse<object> Pay(decimal total, decimal cash)
        {
            _stateMachine.Process(total, cash);

            // kalau gagal
            if (_stateMachine.CurrentState == PaymentState.Failed)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Uang tidak cukup",
                    Data = null
                };
            }

            // hitung kembalian
            decimal change = cash - total;

            // kalau berhasil
            return new ApiResponse<object>
            {
                Success = true,
                Message = "Pembayaran berhasil",
                Data = new
                {
                    Total = total,
                    Bayar = cash,
                    Kembalian = change
                }
            };
        }
    }
}
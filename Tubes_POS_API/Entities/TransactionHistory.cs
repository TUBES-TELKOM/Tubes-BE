using System.ComponentModel.DataAnnotations;

namespace Tubes_POS_API.Entities
{
    public class TransactionHistory
    {
        [Key]
        public int Id { get; set; }

        public int TransactionId { get; set; }

        public DateTime TransactionDate { get; set; }

        public string PaymentMethod { get; set; } = "";

        public decimal TotalAmount { get; set; }
    }
}
namespace ETicketing.Api.Entities
{
    public class LedgerEntry
    {
        public int Id { get; set; }
        public int PaymentTransactionId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string EntryType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public PaymentTransaction PaymentTransaction { get; set; } = null!;
    }
}

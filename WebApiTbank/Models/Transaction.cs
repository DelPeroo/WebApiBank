namespace WebApiTbank.Models
{
    public class Transaction
    {
        public int Id {  get; set; }
        public int SenderWalletId { get; set; }    
        public int ReceiverWalletId { get; set; }
        public decimal Amount {  get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}

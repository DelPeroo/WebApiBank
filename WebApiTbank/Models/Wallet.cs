namespace WebApiTbank.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public decimal Balance { get; set; } = 0.00m;
    }
}

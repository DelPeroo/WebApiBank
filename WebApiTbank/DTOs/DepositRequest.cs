namespace WebApiTbank.DTOs;

public class DepositRequest
{
    public required int WalletId { get; set; }
    public required decimal Amount { get; set; }
}
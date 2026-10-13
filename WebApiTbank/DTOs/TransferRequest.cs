namespace WebApiTbank.DTOs;

public class TransferRequest
{
    public required int SenderWalletId { get; set; }
    public required int ReceiverWalletId { get; set; }
    public required decimal Amount { get; set; }
}
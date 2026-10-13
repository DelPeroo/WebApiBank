using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiTbank.Data;
using WebApiTbank.Models;
using WebApiTbank.DTOs;

namespace WebApiTbank.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WalletsController : ControllerBase
{
    private readonly AppDbContext _context;

    public WalletsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/wallets/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWallet(int id)
    {
        var wallet = await _context.Wallets
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (wallet == null)
        {
            return NotFound("Кошелек не найден.");
        }

        return Ok(wallet);
    }

    // POST: api/wallets/deposit
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Сумма пополнения должна быть больше нуля.");
        }

        var wallet = await _context.Wallets.FindAsync(request.WalletId);
        if (wallet == null)
        {
            return NotFound("Кошелек не найден.");
        }

        wallet.Balance += request.Amount;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Счет успешно пополнен!", NewBalance = wallet.Balance });
    }
    

    // POST: api/wallets/transfer
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Сумма перевода должна быть больше нуля.");
        }

        if (request.SenderWalletId == request.ReceiverWalletId)
        {
            return BadRequest("Нельзя перевести деньги на тот же самый кошелек.");
        }

        // Используем транзакцию БД: гарантирует, что деньги либо спишутся и придут оба раза, либо откатятся назад при ошибке
        using var transactionScope = await _context.Database.BeginTransactionAsync();

        try
        {
            var senderWallet = await _context.Wallets.FindAsync(request.SenderWalletId);
            var receiverWallet = await _context.Wallets.FindAsync(request.ReceiverWalletId);

            if (senderWallet == null || receiverWallet == null)
            {
                return NotFound("Кошелек отправителя или получателя не найден.");
            }

            if (senderWallet.Balance < request.Amount)
            {
                return BadRequest("Недостаточно средств на балансе.");
            }

            // Выполняем перевод
            senderWallet.Balance -= request.Amount;
            receiverWallet.Balance += request.Amount;

            // Записываем транзакцию в историю
            var transaction = new Transaction
            {
                SenderWalletId = request.SenderWalletId,
                ReceiverWalletId = request.ReceiverWalletId,
                Amount = request.Amount,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Подтверждаем транзакцию
            await transactionScope.CommitAsync();

            return Ok(new { Message = "Перевод успешно выполнен!", TransactionId = transaction.Id });
        }

        catch (Exception ex)
        {
            // Если что-то пошло не так, откатываем изменения назад
            await transactionScope.RollbackAsync();
            return StatusCode(500, $"Ошибка при переводе: {ex.Message}");
        }
    }
}
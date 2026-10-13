using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiTbank.Data;
using WebApiTbank.Models;

namespace WebApiTbank.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    
    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/users (Создание нового пользователя и его кошелька)
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest("Имя пользователя не может быть пустым.");
        }
        // 1. Создаем пользователя
        var user = new User { Name = username };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 2. Создаем кошелек для этого пользователя (баланс по умолчанию 0.00)
        var wallet = new Wallet
        {
            UserId = user.Id,
            Balance = 0.00m
        };
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Пользователь и кошелек успешно созданы!", UserId = user.Id, Username = user.Name });
    }

    // GET: api/users (Получить список всех пользователей с их кошельками)
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.Wallet) // Подтягиваем связанный кошелек
            .ToListAsync();

        return Ok(users);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) 
        {
            return NotFound("Пользователь не найден");
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
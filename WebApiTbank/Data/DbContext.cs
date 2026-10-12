using WebApiTbank.Models;
using Microsoft.EntityFrameworkCore;


namespace WebApiTbank.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } // конструктор класса передаем настройки к примеру строку подключения к бд

    // Наши таблицы в базе данных
    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) // метод для личных инструкций перед созданием бд 
    {
        base.OnModelCreating(modelBuilder); //вызывает стандартную логику настройки 

        // Явно указываем точность для decimal (чтобы база знала, как хранить деньги)
        modelBuilder.Entity<Wallet>()
            .Property(w => w.Balance)
            .HasPrecision(18, 2); //до 999 трилионов рублей, 2 знака после запятой

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);
    }
}
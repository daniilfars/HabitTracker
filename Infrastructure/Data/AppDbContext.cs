using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Application.Interfaces;

namespace Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public DbSet<User> Users { get; set;  }
    public DbSet<Habit> Habits { get; set; }
    public DbSet<HabitLog> HabitLogs { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasMaxLength(200);
        modelBuilder.Entity<User>().Property(u => u.Name).HasMaxLength(50);
        modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Habit>().HasIndex(h => new { h.UserId, h.Name }).IsUnique();
        modelBuilder.Entity<Habit>().Property(h => h.Description).HasMaxLength(500);
        modelBuilder.Entity<Habit>().Property(h => h.Name).HasMaxLength(100);
        modelBuilder.Entity<Habit>().Property(h => h.CustomDays).HasColumnType("jsonb");
        modelBuilder.Entity<Habit>().Property(h => h.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Habit>().HasOne(h => h.User).WithMany(u => u.Habits).HasForeignKey(h => h.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Habit>().HasIndex(h => h.UserId);

        modelBuilder.Entity<HabitLog>().HasIndex(l => new { l.HabitId, l.Date }).IsUnique();
        modelBuilder.Entity<HabitLog>().Property(l => l.Date).HasColumnType("date");
        modelBuilder.Entity<HabitLog>().Property(l => l.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<HabitLog>().HasOne(l => l.Habit).WithMany(h => h.HabitLogs).HasForeignKey(l => l.HabitId).OnDelete(DeleteBehavior.Cascade);
    }
}
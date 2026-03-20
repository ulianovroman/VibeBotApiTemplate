using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace VibeBotApi.Storage
{
    public class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<BotContext>
    {
        public BotContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BotContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=test;Username=postgres;Password=postgres");

            return new BotContext(optionsBuilder.Options);
        }
    }
    public class BotContext : DbContext
    {
        public BotContext(DbContextOptions<BotContext> options)
            : base(options)
        {
        }

        public DbSet<UserInStorage> Users => Set<UserInStorage>();
        public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
        public DbSet<MessageLog> MessageLogs => Set<MessageLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserInStorage>()
                .Property(x => x.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<UserPermission>()
                .Property(x => x.UserId)
                .ValueGeneratedNever();
        }
    }
}

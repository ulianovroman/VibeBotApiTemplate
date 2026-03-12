using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace BotApiTemplate.Storage
{
    public class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<WordsToolContext>
    {
        public WordsToolContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WordsToolContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=test;Username=postgres;Password=postgres");

            return new WordsToolContext(optionsBuilder.Options);
        }
    }
    public class WordsToolContext : DbContext
    {
        public WordsToolContext(DbContextOptions<WordsToolContext> options)
            : base(options)
        {
        }

        public DbSet<Word> Words => Set<Word>();
        public DbSet<UserInStorage> Users => Set<UserInStorage>();
        public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
        public DbSet<MessageLog> MessageLogs => Set<MessageLog>();
        public DbSet<UserLanguage> UserLanguages => Set<UserLanguage>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<UserCard> UserCards => Set<UserCard>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserInStorage>()
                .Property(x => x.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<UserPermission>()
                .Property(x => x.UserId)
                .ValueGeneratedNever();

            modelBuilder.Entity<UserLanguage>()
                .HasKey(x => new { x.UserId, x.NativeLang, x.StudyLang });

            modelBuilder.Entity<UserCard>()
                .HasKey(x => new { x.UserId, x.StackId, x.CardId });

            modelBuilder.Entity<UserCard>()
                .HasOne<Card>()
                .WithMany()
                .HasForeignKey(x => x.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using File = Infrastructure.AFC.Infrastructure.Database.Entities.File;
using Infrastructure.AFC.Infrastructure.Database.Entities;
namespace Infrastructure.AFC.Infrastructure.Database
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<GroupConnection> GroupConnections { get; set; }
        public DbSet<Message> Messagess { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connection = "Host=localhost;Port=5432;Database=ChatDb;Username=postgres;Password=gc2gKosdYI2i8OCsyYUn;";
            optionsBuilder.UseNpgsql(connection);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(a => a.OwnedGroups)
                .WithOne(b => b.Owner)
                .HasForeignKey(b => b.OwnerId);

            modelBuilder.Entity<User>()
                .HasMany(a => a.BeInGroups)
                .WithMany(b => b.Members);
        }
    }
}

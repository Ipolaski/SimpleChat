using Microsoft.EntityFrameworkCore;
using AFC.Infrastructure.chat.Database;
using System.Configuration;

namespace AFC.database.chat
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Infrastructure.chat.Database.File> Files { get; set; }
        public DbSet<GroupConnection> GroupConnections { get; set; }
        public DbSet<Messages> Messagess { get; set; }

        public ApplicationContext() 
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConfigurationManager.AppSettings["PostgressConnectionString"]);
            base.OnConfiguring(optionsBuilder);
        }
    }
}

using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace common
{
    public partial class Context : DbContext
    {
        private bool useInMemoryDatabase = false;
        private bool useConfiguration = false;
        public Context()
        {

        }
        public Context(bool useInMemoryDatabase)
        {
            this.useInMemoryDatabase = useInMemoryDatabase;
            this.useConfiguration = true;
        }
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }
        // public virtual DbSet<User> users { get; set; }
        // public virtual DbSet<AdminInst> admin_insts { get; set; }
        // public virtual DbSet<Profile> profile { get; set; }
        public virtual DbSet<Admin> admins { get; set; }

        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (useInMemoryDatabase)
                optionsBuilder.UseInMemoryDatabase("menu");
            optionsBuilder.EnableSensitiveDataLogging();
            if (useConfiguration) {
                if (!optionsBuilder.IsConfigured) {
                    optionsBuilder.UseMySql(databaseConnection());
                }
            }
        }
        public static string databaseConnection()
        {
            var serverConfig = Program.serverConfiguration();
            var sqlConfig = serverConfig.GetSection("database");
            return "Server=" + sqlConfig.GetValue<string>("Server") +
                ";Database=" + sqlConfig.GetValue<string>("Database") + 
                ";User=" + sqlConfig.GetValue<string>("User") + 
                ";Pwd=" + sqlConfig.GetValue<string>("Password") + 
                ";Charset=utf8;";
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}

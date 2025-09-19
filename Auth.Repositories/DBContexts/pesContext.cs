#nullable disable
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace Auth.Infrastructure.DBContexts
{
    public partial class pesContext : DbContext
    {
        public pesContext()
        {
        }

        public pesContext(DbContextOptions<pesContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; } 

        public static string GetConnectionString(string connectionStringName)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = config.GetConnectionString(connectionStringName);
            return connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(GetConnectionString("DefaultConnection"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Address).HasMaxLength(150).HasColumnName("address");
                entity.Property(e => e.AvatarUrl).HasMaxLength(255).HasColumnName("avatar_url");
                entity.Property(e => e.CreatedAt).HasPrecision(6).HasColumnName("created_at");
                entity.Property(e => e.Email).HasMaxLength(150).HasColumnName("email");
                entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("name");
                entity.Property(e => e.Phone).HasMaxLength(11).HasColumnName("phone");
                entity.Property(e => e.Role).HasMaxLength(20).HasColumnName("role");
                entity.Property(e => e.Status).HasMaxLength(50).HasColumnName("status");
                entity.Property(e => e.Gender).HasMaxLength(10).HasColumnName("gender");
                entity.Property(e => e.AvatarUrl).HasMaxLength(255).HasColumnName("avatar_url");
                entity.Property(e => e.PasswordHash)
                      .HasMaxLength(255)
                      .HasColumnName("password");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

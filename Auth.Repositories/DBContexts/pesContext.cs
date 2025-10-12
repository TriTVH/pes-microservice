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

        public virtual DbSet<Domain.Entities.Account> Accounts { get; set; }
        public virtual DbSet<Activity> Activities { get; set; }

        public virtual DbSet<Class> Classes { get; set; }

        public virtual DbSet<Schedule> Schedules { get; set; }
        public virtual DbSet<Parent> Parents { get; set; }
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
            modelBuilder.Entity<Domain.Entities.Account>(entity =>
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
            modelBuilder.Entity<Activity>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__activity__3213E83F6EACBA9C");

                entity.ToTable("activity");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Date).HasColumnName("date");
                entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
                entity.Property(e => e.EndTime)
                    .HasPrecision(6)
                    .HasColumnName("end_time");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");
                entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
                entity.Property(e => e.StartTime)
                    .HasPrecision(6)
                    .HasColumnName("start_time");
               

                entity.HasOne(d => d.Schedule).WithMany(p => p.Activities)
                    .HasForeignKey(d => d.ScheduleId)
                    .HasConstraintName("FK_activity_schedule");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__classes__3213E83F045BB703");

                entity.ToTable("classes");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.AcademicYear).HasColumnName("academic_year");
                entity.Property(e => e.EndDate).HasColumnName("end_date");
             
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");
                entity.Property(e => e.NumberStudent).HasColumnName("number_student");
                entity.Property(e => e.StartDate).HasColumnName("start_date");
                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasColumnName("status");
                entity.Property(e => e.SyllabusId).HasColumnName("syllabus_id");
                entity.Property(e => e.TeacherId).HasColumnName("teacher_id");


                entity.HasOne(d => d.Teacher).WithMany(p => p.Classes)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("FK_classes_teacher");
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__schedule__3213E83F36060861");

                entity.ToTable("schedule");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ClassesId).HasColumnName("classes_id");
                entity.Property(e => e.WeekName)
                    .HasMaxLength(50)
                    .HasColumnName("week_name");

                entity.HasOne(d => d.Classes).WithMany(p => p.Schedules)
                    .HasForeignKey(d => d.ClassesId)
                    .HasConstraintName("FK_schedule_classes");
            });
            modelBuilder.Entity<Parent>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__parent__3213E83FA5766047");

                entity.ToTable("parent");

                entity.HasIndex(e => e.AccountId, "UQ__parent__46A222CCFEC1B2DB").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.AccountId).HasColumnName("account_id");
                entity.Property(e => e.Job)
                    .HasMaxLength(100)
                    .HasColumnName("job");
                entity.Property(e => e.RelationshipToChild)
                    .HasMaxLength(50)
                    .HasColumnName("relationship_to_child");
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using TermAdmissionManagement.Infrastructure.Entities;

namespace TermAdmissionManagement.Infrastructure.DBContext;

public partial class PesTermManagementContext : DbContext
{
    public PesTermManagementContext()
    {
    }

    public PesTermManagementContext(DbContextOptions<PesTermManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdmissionTerm> AdmissionTerms { get; set; }

    public virtual DbSet<TermItem> TermItems { get; set; }

  
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdmissionTerm>(entity =>
        {
            entity.ToTable("AdmissionTerm");

            entity.Property(e => e.Id)
                .HasColumnName("id").UseIdentityColumn();
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("endDate");
            entity.Property(e => e.Name)
                .HasMaxLength(254)
                .HasColumnName("name");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("startDate");
        });

        modelBuilder.Entity<TermItem>(entity =>
        {
            entity.ToTable("TermItem");

            entity.Property(e => e.Id).UseIdentityColumn()
                .HasColumnName("id");
            entity.Property(e => e.AdmissionTermId).HasColumnName("admissionTermId");
            entity.Property(e => e.CurrentRegisteredStudents).HasColumnName("currentRegisteredStudents");
            entity.Property(e => e.ExpectedClasses).HasColumnName("expectedClasses");
            entity.Property(e => e.Grade)
                .HasMaxLength(50)
                .HasColumnName("grade");
            entity.Property(e => e.MaxNumberRegistration).HasColumnName("maxNumberRegistration");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.AdmissionTerm).WithMany(p => p.TermItems)
                .HasForeignKey(d => d.AdmissionTermId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TermItem_AdmissionTerm");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

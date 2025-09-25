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

    public virtual DbSet<AdmissionForm> AdmissionForms { get; set; }
  
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
            entity.Property(e => e.DefaultFee).HasColumnName("defaultFee");
            entity.Property(e => e.MaxNumberRegistration).HasColumnName("maxNumberRegistration");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.AdmissionTerm).WithMany(p => p.TermItems)
                .HasForeignKey(d => d.AdmissionTermId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TermItem_AdmissionTerm");
        });

        modelBuilder.Entity<AdmissionForm>(entity =>
        { 
            entity.ToTable("admission_form");

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
            entity.Property(e => e.ApprovedDate)
                .HasPrecision(6)
                .HasColumnName("approved_date");
            entity.Property(e => e.CancelReason)
                .HasMaxLength(255)
                .HasColumnName("cancel_reason");
            entity.Property(e => e.ChildCharacteristicsFormImg)
                .HasMaxLength(255)
                .HasColumnName("child_characteristics_form_img");
            entity.Property(e => e.CommitmentImg)
                .HasMaxLength(255)
                .HasColumnName("commitment_img");
            entity.Property(e => e.HouseholdRegistrationAddress)
                .HasMaxLength(255)
                .HasColumnName("household_registration_address");
            entity.Property(e => e.Note)
                .HasMaxLength(50)
                .HasColumnName("note");
            entity.Property(e => e.ParentAccountId).HasColumnName("parent_account_id");
            entity.Property(e => e.PaymentExpiryDate)
                .HasPrecision(6)
                .HasColumnName("payment_expiry_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SubmittedDate)
                .HasPrecision(6)
                .HasColumnName("submitted_date");
            entity.Property(e => e.TermItemId).HasColumnName("term_item_id");

            entity.HasOne(d => d.TermItem)
            .WithMany(p => p.AdmissionForms)
            .HasForeignKey(d => d.TermItemId)   // <- đúng: FK property
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_admission_form_TermItem");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

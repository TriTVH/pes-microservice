using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SyllabusService.Infrastructure.DBContext;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unit.Tests.Repositories
{
    public class AdmissionFormRepoTest
    {
        private readonly PES_APP_FULL_DBContext _context;
        private readonly AdmissionFormRepo _repository;

        public AdmissionFormRepoTest()
        {
            var options = new DbContextOptionsBuilder<PES_APP_FULL_DBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging() // giúp debug dễ hơn
                .Options;

            _context = new PES_APP_FULL_DBContext(options);
            _context.Database.EnsureCreated();

            _repository = new AdmissionFormRepo(_context);
        }

        [Fact]
        public async Task UpdateAdmissionFormAsync_ShouldUpdateSuccessfully()
        {
            // Arrange
            var form = new AdmissionForm
            {
                Id = 1,
                Status = "waiting_for_payment",
                AdmissionTermId = 1
            };
            _context.AdmissionForms.Add(form);
            await _context.SaveChangesAsync();

            // Act
            form.Status = "approved";
            var result = await _repository.UpdateAdmissionFormAsync(form);

            // Assert
            result.Should().Be(1);
            (await _context.AdmissionForms.FindAsync(1))!.Status.Should().Be("approved");
        }

        [Fact]
        public async Task GetAdmissionFormByIdAsync_ShouldReturnCorrectForm()
        {
            var form = new AdmissionForm { Id = 2, Status = "waiting_for_payment", AdmissionTermId = 1 };
            _context.AdmissionForms.Add(form);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAdmissionFormByIdAsync(2);
            result.Should().NotBeNull();
            result!.Id.Should().Be(2);
        }

        [Fact]
        public async Task GetAdmissionFormsByAdmissionTermIdAsync_ShouldReturnSortedList()
        {
            // Arrange
            var termId = 100;
            var forms = new List<AdmissionForm>
            {
                new AdmissionForm
                {
                    Id = 3,
                    AdmissionTermId = termId,
                    Status = "approved",
                    SubmittedDate = DateTime.UtcNow.AddDays(-2)
                },
                new AdmissionForm
                {
                    Id = 4,
                    AdmissionTermId = termId,
                    Status = "waiting_for_approve",
                    SubmittedDate = DateTime.UtcNow
                },
                new AdmissionForm
                {
                    Id = 5,
                    AdmissionTermId = termId,
                    Status = "waiting_for_approve",
                    SubmittedDate = DateTime.UtcNow.AddHours(-2)
                }
            };
            _context.AdmissionForms.AddRange(forms);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAdmissionFormsByAdmissionTermIdAsync(termId);

            // Assert
            result.Should().HaveCount(3);
            var ordered = result.ToList();
            ordered[0].Status.Should().Be("waiting_for_approve");
        }

        [Fact]
        public async Task UpdateAdmissionFormsOverDueDateAuto_ShouldUpdateOverdueStatuses()
        {
            // Arrange
            var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            var pastTerm = new AdmissionTerm
            {
                Id = 1,
                EndDate = vnNow.AddDays(-2),
                StartDate = vnNow.AddDays(-10),
                Status = "inactive"
            };

            var activeTerm = new AdmissionTerm
            {
                Id = 2,
                EndDate = vnNow.AddDays(5),
                StartDate = vnNow.AddDays(-1),
                Status = "active"
            };

            var forms = new List<AdmissionForm>
            {
                new AdmissionForm
                {
                    Id = 10,
                    Status = "waiting_for_payment",
                    AdmissionTerm = pastTerm
                },
                new AdmissionForm
                {
                    Id = 11,
                    Status = "waiting_for_approve",
                    AdmissionTerm = pastTerm
                },
                new AdmissionForm
                {
                    Id = 12,
                    Status = "approved",
                    AdmissionTerm = pastTerm // không được đổi vì không nằm trong target statuses
                },
                new AdmissionForm
                {
                    Id = 13,
                    Status = "waiting_for_payment",
                    AdmissionTerm = activeTerm // chưa hết hạn, không đổi
                }
            };

            _context.AdmissionForms.AddRange(forms);
            await _context.SaveChangesAsync();

            // Act
            await _repository.UpdateAdmissionFormsOverDueDateAuto();

            // Assert
            var updated = await _context.AdmissionForms
                .Where(f => f.Status == "over_due_date")
                .ToListAsync();

            updated.Should().HaveCount(2); // Id 10 và 11
            updated.Select(f => f.Id).Should().Contain(new[] { 10, 11 });
        }

        [Fact]
        public async Task UpdateAdmissionFormsOverDueDateAuto_ShouldDoNothing_WhenNoOverdueForms()
        {
            var term = new AdmissionTerm
            {
                Id = 3,
                EndDate = DateTime.UtcNow.AddDays(10),
                StartDate = DateTime.UtcNow.AddDays(-1),
                Status = "active"
            };

            var forms = new List<AdmissionForm>
            {
                new AdmissionForm { Id = 20, Status = "approved", AdmissionTerm = term },
                new AdmissionForm { Id = 21, Status = "rejected", AdmissionTerm = term }
            };
            _context.AdmissionForms.AddRange(forms);
            await _context.SaveChangesAsync();

            await _repository.UpdateAdmissionFormsOverDueDateAuto();

            var countOverdue = await _context.AdmissionForms.CountAsync(f => f.Status == "over_due_date");
            countOverdue.Should().Be(0);
        }
    }
}

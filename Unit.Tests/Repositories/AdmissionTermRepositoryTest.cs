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
    public class AdmissionTermRepositoryTest
    {
        private readonly PES_APP_FULL_DBContext _context;
        private readonly AdmissionTermRepository _repository;

        public AdmissionTermRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<PES_APP_FULL_DBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
                .Options;

            _context = new PES_APP_FULL_DBContext(options);
            _context.Database.EnsureCreated();

            _repository = new AdmissionTermRepository(_context);
        }

        [Fact]
        public async Task CreateAdmissionTermAsync_ShouldAddSuccessfully()
        {
            var term = new AdmissionTerm
            {
                Id = 1,
                AcdemicYear = 2025,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(10),
                Status = "active"
            };

            var result = await _repository.CreateAdmissionTermAsync(term);

            result.Should().Be(1);
            (await _context.AdmissionTerms.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task UpdateAdmissionTermAsync_ShouldModifySuccessfully()
        {
            var term = new AdmissionTerm
            {
                Id = 2,
                AcdemicYear = 2025,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                Status = "inactive"
            };

            _context.AdmissionTerms.Add(term);
            await _context.SaveChangesAsync();

            term.Status = "active";
            var result = await _repository.UpdateAdmissionTermAsync(term);

            result.Should().Be(1);
            (await _context.AdmissionTerms.FindAsync(2))!.Status.Should().Be("active");
        }

        [Fact]
        public async Task GetAdmissionTermsAsync_ShouldIncludeRelations()
        {
            _context.AdmissionTerms.Add(new AdmissionTerm
            {
                Id = 3,
                AcdemicYear = 2025,
                Status = "active",
                Classes = new List<Class>
                {
                    new Class
                    {
                        Id = 100,
                        Name = "PRN222",
                        Syllabus = new Syllabus { Id = 10, Name = "C#", Cost = 100000 }
                    }
                }
            });
            await _context.SaveChangesAsync();

            var result = await _repository.GetAdmissionTermsAsync();

            result.Should().HaveCount(1);
            result.First().Classes.Should().ContainSingle();
            result.First().Classes.First().Syllabus.Name.Should().Be("C#");
        }

        [Fact]
        public async Task GetActiveAdmissionTerm_ShouldReturnActive()
        {
            _context.AdmissionTerms.AddRange(
                new AdmissionTerm
                {
                    Id = 4,
                    AcdemicYear = 2025,
                    Status = "inactive",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(2)
                },
                new AdmissionTerm
                {
                    Id = 5,
                    AcdemicYear = 2025,
                    Status = "active",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(5)
                }
            );
            await _context.SaveChangesAsync();

            var result = await _repository.GetActiveAdmissionTerm();

            result.Should().NotBeNull();
            result!.Status.Should().Be("active");
        }

        [Fact]
        public async Task GetOverlappingTermAsync_ShouldReturnOverlap()
        {
            var existing = new AdmissionTerm
            {
                Id = 6,
                AcdemicYear = 2025,
                Status = "active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(10)
            };
            _context.AdmissionTerms.Add(existing);
            await _context.SaveChangesAsync();

            var overlap = await _repository.GetOverlappingTermAsync(DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(8));

            overlap.Should().NotBeNull();
            overlap!.Id.Should().Be(6);
        }

        [Fact]
        public async Task GetOverlappingTermAsyncExceptId_ShouldExcludeId()
        {
            _context.AdmissionTerms.AddRange(
                new AdmissionTerm
                {
                    Id = 7,
                    AcdemicYear = 2025,
                    Status = "active",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(5)
                },
                new AdmissionTerm
                {
                    Id = 8,
                    AcdemicYear = 2025,
                    Status = "active",
                    StartDate = DateTime.UtcNow.AddDays(6),
                    EndDate = DateTime.UtcNow.AddDays(10)
                }
            );
            await _context.SaveChangesAsync();

            var result = await _repository.GetOverlappingTermAsyncExceptId(
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(4),
                7
            );

            result.Should().BeNull(); // excluded Id 7
        }

        [Fact]
        public async Task GetAdmissionTermByIdAsync_ShouldReturnTerm()
        {
            _context.AdmissionTerms.Add(new AdmissionTerm
            {
                Id = 9,
                AcdemicYear = 2025,
                Status = "inactive"
            });
            await _context.SaveChangesAsync();

            var result = await _repository.GetAdmissionTermByIdAsync(9);

            result.Should().NotBeNull();
            result!.Id.Should().Be(9);
        }

        [Fact]
        public async Task GetPrioritizedAdmissionTermsAsync_ShouldOrderCorrectly()
        {
            _context.AdmissionTerms.AddRange(
                new AdmissionTerm
                {
                    Id = 10,
                    AcdemicYear = 2025,
                    Status = "inactive",
                    StartDate = DateTime.UtcNow.AddDays(1)
                },
                new AdmissionTerm
                {
                    Id = 11,
                    AcdemicYear = 2025,
                    Status = "active",
                    StartDate = DateTime.UtcNow.AddDays(2)
                },
                new AdmissionTerm
                {
                    Id = 12,
                    AcdemicYear = 2025,
                    Status = "pending",
                    StartDate = DateTime.UtcNow.AddDays(3)
                }
            );
            await _context.SaveChangesAsync();

            var result = (await _repository.GetPrioritizedAdmissionTermsAsync()).ToList();

            result.Should().HaveCount(2); // inactive bị loại
            result.First().Status.Should().Be("active");
        }

        [Fact]
        public async Task UpdateStatusAuto_ShouldUpdateStatusesCorrectly()
        {
            var now = DateTime.UtcNow.AddHours(7);

            _context.AdmissionTerms.AddRange(
                new AdmissionTerm
                {
                    Id = 20,
                    AcdemicYear = 2025,
                    StartDate = now.AddDays(1),
                    EndDate = now.AddDays(5),
                    Status = "active" // should become inactive
                },
                new AdmissionTerm
                {
                    Id = 21,
                    AcdemicYear = 2025,
                    StartDate = now.AddDays(-1),
                    EndDate = now.AddDays(2),
                    Status = "inactive" // should become active
                },
                new AdmissionTerm
                {
                    Id = 22,
                    AcdemicYear = 2025,
                    StartDate = now.AddDays(-10),
                    EndDate = now.AddDays(-5),
                    Status = "active" // should become blocked
                }
            );
            await _context.SaveChangesAsync();

            await _repository.UpdateStatusAuto();


            var terms = await _context.AdmissionTerms.OrderBy(t => t.Id).ToListAsync();

            terms[0].Status.Should().Be("inactive");
            terms[1].Status.Should().Be("active");
            terms[2].Status.Should().Be("blocked");
        }
    }
}

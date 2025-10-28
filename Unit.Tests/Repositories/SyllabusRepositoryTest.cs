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
    public class SyllabusRepositoryTest
    {
        private readonly PES_APP_FULL_DBContext _context;
        private readonly SyllabusRepository _repository;

        public SyllabusRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<PES_APP_FULL_DBContext>()
                .UseInMemoryDatabase(databaseName: "SyllabusRepoDB")
                .Options;

            _context = new PES_APP_FULL_DBContext(options);
            _context.Database.EnsureDeleted(); // reset DB before each test
            _context.Database.EnsureCreated();

            _repository = new SyllabusRepository(_context);
        }

        [Fact]
        public async Task CreateSyllabusAsync_ShouldAddSyllabus()
        {
            // Arrange
            var syllabus = new Syllabus
            {
                Id = 1,
                Name = "PRN222",
                Description = "C# .NET",
                Cost = 200000,
                HoursOfSyllabus = 40,
                IsActive = true
            };

            // Act
            var result = await _repository.CreateSyllabusAsync(syllabus);

            // Assert
            result.Should().Be(1);
            (await _context.Syllabi.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task GetAllSyllabusAsync_ShouldReturnAll()
        {
            // Arrange
            _context.Syllabi.AddRange(
                new Syllabus { Id = 1, Name = "PRN222", IsActive = true },
                new Syllabus { Id = 2, Name = "DBI202", IsActive = false });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllSyllabusAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllActiveSyllabusAsync_ShouldReturnOnlyActive()
        {
            // Arrange
            _context.Syllabi.AddRange(
                new Syllabus { Id = 1, Name = "PRN222", IsActive = true },
                new Syllabus { Id = 2, Name = "DBI202", IsActive = false });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllActiveSyllabusAsync();

            // Assert
            result.Should().ContainSingle(s => s.Name == "PRN222");
        }

        [Fact]
        public async Task GetSyllabusByIdAsync_ShouldReturnCorrectSyllabus()
        {
            // Arrange
            var syllabus = new Syllabus { Id = 5, Name = "OOP" };
            _context.Syllabi.Add(syllabus);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSyllabusByIdAsync(5);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("OOP");
        }

        [Fact]
        public async Task GetSyllabusByNameAsync_ShouldReturnCorrectSyllabus()
        {
            // Arrange
            var syllabus = new Syllabus { Id = 10, Name = "PRN231" };
            _context.Syllabi.Add(syllabus);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSyllabusByNameAsync("PRN231");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(10);
        }

        [Fact]
        public async Task UpdateSyllabusAsync_ShouldModifySyllabus()
        {
            // Arrange
            var syllabus = new Syllabus { Id = 7, Name = "OLD_NAME", IsActive = true };
            _context.Syllabi.Add(syllabus);
            await _context.SaveChangesAsync();

            syllabus.Name = "NEW_NAME";

            // Act
            var result = await _repository.UpdateSyllabusAsync(syllabus);

            // Assert
            result.Should().Be(1);
            (await _context.Syllabi.FindAsync(7))!.Name.Should().Be("NEW_NAME");
        }

        [Fact]
        public async Task IsDuplicateNameAsync_ShouldReturnTrue_WhenDuplicateExists()
        {
            // Arrange
            _context.Syllabi.AddRange(
                new Syllabus { Id = 1, Name = "PRN222" },
                new Syllabus { Id = 2, Name = "PRN231" });
            await _context.SaveChangesAsync();

            // Act
            var resultTrue = await _repository.IsDuplicateNameAsync("PRN222", 2);
            var resultFalse = await _repository.IsDuplicateNameAsync("PRN999", 3);

            // Assert
            resultTrue.Should().BeTrue();
            resultFalse.Should().BeFalse();
        }
    }
}

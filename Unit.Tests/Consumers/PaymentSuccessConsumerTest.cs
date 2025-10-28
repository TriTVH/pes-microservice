using Contracts;
using FluentAssertions;
using MassTransit;
using Moq;
using SyllabusService.Application.Consumers;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unit.Tests.Consumers
{
    public class PaymentSuccessConsumerTest
    {
        private readonly Mock<IClassRepository> _mockClassRepo;
        private readonly Mock<IAdmissionTermRepo> _mockTermRepo;
        private readonly Mock<ConsumeContext<PaymentSuccessEvent>> _mockContext;
        private readonly PaymentSuccessConsumer _consumer;

        public PaymentSuccessConsumerTest()
        {
            _mockClassRepo = new Mock<IClassRepository>();
            _mockTermRepo = new Mock<IAdmissionTermRepo>();
            _mockContext = new Mock<ConsumeContext<PaymentSuccessEvent>>();


            _consumer = new PaymentSuccessConsumer(
                _mockClassRepo.Object,
                _mockTermRepo.Object
            );
        }

        [Fact]
        public async Task Consume_ShouldHandleClassesWithAvailableAndFullSpots()
        {
            // Arrange
            var admissionTerm = new AdmissionTerm
            {
                Id = 1,
                CurrentRegisteredStudents = 5
            };

            var clsFull = new Class { Id = 1, NumberStudent = 30 };
            var clsAvailable = new Class { Id = 2, NumberStudent = 25 };

            var message = new PaymentSuccessEvent
            {
                AdmissionFormId = 123,
                ClassIds = new List<int> { 1, 2 },
                TxnRef = "TX12345",
                Amount = 500000
            };

            _mockContext.Setup(x => x.Message).Returns(message);

            _mockTermRepo.Setup(x => x.GetActiveAdmissionTerm())
                .ReturnsAsync(admissionTerm);

            _mockClassRepo.Setup(x => x.GetClassByIdAsync(1)).ReturnsAsync(clsFull);
            _mockClassRepo.Setup(x => x.GetClassByIdAsync(2)).ReturnsAsync(clsAvailable);

            _mockClassRepo.Setup(x => x.UpdateClassAsync(It.IsAny<Class>())).ReturnsAsync(1);
            _mockTermRepo.Setup(x => x.UpdateAdmissionTermAsync(It.IsAny<AdmissionTerm>())).ReturnsAsync(1);

        

            // Act
            await _consumer.Consume(_mockContext.Object);

            // Assert
            _mockClassRepo.Verify(x => x.UpdateClassAsync(It.Is<Class>(c => c.Id == 2)), Times.Once);
            _mockTermRepo.Verify(x => x.UpdateAdmissionTermAsync(admissionTerm), Times.Once);

            admissionTerm.CurrentRegisteredStudents.Should().Be(6);
            clsAvailable.NumberStudent.Should().Be(26);

        }


        [Fact]
        public async Task Consume_ShouldUpdateClassesAndAdmissionTerm_WhenAllClassesAvailable()
        {
            // Arrange
            var admissionTerm = new AdmissionTerm { Id = 2, CurrentRegisteredStudents = 0 };
            var cls1 = new Class { Id = 10, NumberStudent = 15 };
            var cls2 = new Class { Id = 11, NumberStudent = 28 };

            var message = new PaymentSuccessEvent
            {
                AdmissionFormId = 456,
                ClassIds = new List<int> { 10, 11 },
                TxnRef = "TX6789",
                Amount = 600000
            };

            var mockContext = new Mock<ConsumeContext<PaymentSuccessEvent>>();
            mockContext.Setup(x => x.Message).Returns(message);

            _mockTermRepo.Setup(x => x.GetActiveAdmissionTerm()).ReturnsAsync(admissionTerm);
            _mockClassRepo.Setup(x => x.GetClassByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => id == 10 ? cls1 : cls2);
            _mockClassRepo.Setup(x => x.UpdateClassAsync(It.IsAny<Class>())).ReturnsAsync(1);
            _mockTermRepo.Setup(x => x.UpdateAdmissionTermAsync(It.IsAny<AdmissionTerm>())).ReturnsAsync(1);

            // Act
            await _consumer.Consume(mockContext.Object);

            // Assert
            _mockClassRepo.Verify(x => x.UpdateClassAsync(It.IsAny<Class>()), Times.Exactly(2));
            _mockTermRepo.Verify(x => x.UpdateAdmissionTermAsync(It.IsAny<AdmissionTerm>()), Times.Once);

            admissionTerm.CurrentRegisteredStudents.Should().Be(2);
            cls1.NumberStudent.Should().Be(16);
            cls2.NumberStudent.Should().Be(29);
        }


    }
}

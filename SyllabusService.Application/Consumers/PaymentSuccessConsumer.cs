using Contracts;
using MassTransit;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Application.Consumers
{
    public class PaymentSuccessConsumer : IConsumer<PaymentSuccessEvent>
    {
        private readonly IClassRepository _classRepo;
        private readonly IAdmissionTermRepo _admissionTermRepo;
        public PaymentSuccessConsumer(IClassRepository classRepo, IAdmissionTermRepo admissionTermRepo)
        {
            _classRepo = classRepo;
            _admissionTermRepo = admissionTermRepo;
        }

        public async Task Consume(ConsumeContext<PaymentSuccessEvent> context)
        {
            var msg = context.Message;
            var success = new List<int>();
            var failed = new List<int>();

            var admissionTerm = await _admissionTermRepo.GetActiveAdmissionTerm();
            foreach (var classId in msg.ClassIds)
            {
                var cls = await _classRepo.GetClassByIdAsync(classId);

                if (cls.NumberStudent >= 30)
                {
                    failed.Add(classId);
                    continue;
                }

                cls.NumberStudent++;
                success.Add(classId);

                await _classRepo.UpdateClassAsync(cls);
                admissionTerm.CurrentRegisteredStudents++;
            }

            await _admissionTermRepo.UpdateAdmissionTermAsync(admissionTerm);
            
            // ✅ Gửi phản hồi lại cho ParentService
            await context.RespondAsync(new ClassProcessResultEvent
            {
                AdmissionFormId = msg.AdmissionFormId,
                SuccessfulClassIds = success,
                FailedClassIds = failed,
                TxnRef = msg.TxnRef,
                Amount = msg.Amount,
                Reason = failed.Any() ? $"Classes full: {string.Join(", ", failed)}" : "All OK"
            });

        }
    }
}
